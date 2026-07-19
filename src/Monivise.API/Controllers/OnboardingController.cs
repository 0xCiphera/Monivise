using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monivise.Application.DTOs.Onboarding;
using Monivise.Application.Interfaces.Repositories;
using Monivise.Application.Interfaces.Services;
using Monivise.Domain.Entities;
using Monivise.Domain.Enums;

namespace Monivise.API.Controllers
{
    [Authorize]
    [Route("api/onboarding")]
    public class OnboardingController(
    IIntakeProfileRepository intakes,
    IBucketRepository buckets,
    IBudgetCycleRepository cycles,
    ITransactionRepository transactions,
    IFinancialCalculationService calc,
    IAuditLogRepository audit,
    IWantCategoryRepository wantCategories,
    IFixedObligationStatusRepository fixedObligationStatuses,
    IAllocationRecommendationService recommender) : ApiControllerBase
    {
        /// <summary>Save intake + baseline, return 3 pathway previews.</summary>
        [HttpPost("intake")]
        public async Task<IActionResult> SubmitIntake([FromBody] SubmitIntakeDto dto, CancellationToken ct)
        {
            if (dto.Items.Any(i => i.Nature == "Unpriced"))
                return UnprocessableEntity(new
                {
                    code = "UNPRICED_NOT_ALLOWED",
                    detail = "Only Want categories can be left unpriced. Give every Fixed Obligation, Flexible Spending, and Investment item a real amount."
                });

            if (dto.WantCategories.Count == 0)
                return UnprocessableEntity(new
                {
                    code = "WANTS_REQUIRED",
                    detail = "At least one Want category is required to finish onboarding."
                });

            decimal committed = dto.Items.Sum(i => i.MonthlyAmount)
                + dto.WantCategories.Where(w => !w.IsUnpriced).Sum(w => w.MonthlyAmount);

            if (committed > dto.BaselineIncome)
                return UnprocessableEntity(new
                {
                    code = "INTAKE_EXCEEDS_INCOME",
                    committed,
                    income = dto.BaselineIncome,
                    gap = Math.Round(committed - dto.BaselineIncome, 2)
                });

            var existing = await intakes.GetByUserIdAsync(UserId, ct);
            IntakeProfile profile;
            if (existing is null)
            {
                profile = IntakeProfile.Create(UserId, dto.BaselineIncome);
                await intakes.AddAsync(profile, ct);
            }
            else
            {
                profile = existing;
                profile.UpdateBaseline(dto.BaselineIncome);
                var staleItems = profile.Items.ToList();
                await intakes.DeleteItemsByProfileIdAsync(profile.Id, ct);
                intakes.DetachItems(staleItems);
                profile.ClearItems();
            }

            foreach (var i in dto.Items)
                profile.AddItem(IntakeItem.Create(profile.Id, i.Name,
                    Enum.Parse<ExpenseCategory>(i.Category),
                    Enum.Parse<ItemNature>(i.Nature),
                    i.MonthlyAmount, i.ReserveOnly));

            // Want categories persist independently of the intake profile — they're
            // referenced long-term by Transactions, not just used to compute a preview.
            await wantCategories.DeactivateAllForUserAsync(UserId, ct);
            int order = 0;
            var newWants = dto.WantCategories
                .Select(w => WantCategory.Create(UserId, w.Name, w.IsUnpriced, w.MonthlyAmount, order++))
                .ToList();
            await wantCategories.AddRangeAsync(newWants, ct);

            await intakes.SaveChangesAsync(ct);
            await wantCategories.SaveChangesAsync(ct);

            var activeWants = await wantCategories.GetActiveByUserIdAsync(UserId, ct);
            return Ok(recommender.BuildPathways(profile, activeWants));
        }

        /// <summary>Choose a pathway → seed buckets → record baseline income → complete onboarding.</summary>
        [HttpPost("commit")]
        public async Task<IActionResult> Commit([FromBody] CommitPathwayDto dto, CancellationToken ct)
        {

            var activeCycle = await cycles.GetActiveByUserIdAsync(UserId, ct);
            if (activeCycle is not null)
                return Conflict(new { code = "CYCLE_ALREADY_ACTIVE", detail = "You already have an active budget cycle — re-onboarding isn't available mid-cycle yet." });

            var profile = await intakes.GetByUserIdAsync(UserId, ct)
                ?? throw new ArgumentException("No intake profile. Submit intake first.");

            var activeWants = (await wantCategories.GetActiveByUserIdAsync(UserId, ct)).ToList();

            var chosen = Enum.Parse<PathwayType>(dto.Pathway);
            var preview = recommender.BuildPathways(profile, activeWants).First(p => p.Pathway == dto.Pathway);
            if (!preview.IsAffordable)
                return UnprocessableEntity(new { code = "PATHWAY_UNAFFORDABLE", preview.AffordabilityGap });

            var seededBuckets = new List<Bucket>();
            int order = 0;
            foreach (var b in preview.Buckets)
            {
                var bucket = Bucket.Create(UserId, b.Name, "💰", "#00CFA8",
                    Enum.Parse<BucketType>(b.Type), b.AllocationPercent, order++);
                seededBuckets.Add(bucket);
                await buckets.AddAsync(bucket, ct);
            }

            profile.ChoosePathway(chosen);

            var cycle = BudgetCycle.CreateCurrentMonth(UserId);
            cycle.SeedBuffer(preview.BufferAmount);
            cycle.SeedUnpricedPool(preview.UnpricedWantsPoolAmount);
            await cycles.AddAsync(cycle, ct);

            await buckets.SaveChangesAsync(ct);
            await cycles.SaveChangesAsync(ct);

            // Seed the paid-checklist — one row per Fixed Obligation item, unpaid, for this cycle.
            var fixedItems = profile.Items.Where(i => i.Nature == ItemNature.HardFixed
                && i.Category != Monivise.Domain.Enums.ExpenseCategory.Investment).ToList();
            if (fixedItems.Count > 0)
            {
                var statuses = fixedItems.Select(i => FixedObligationStatus.Create(cycle.Id, i.Id));
                await fixedObligationStatuses.AddAsync(statuses, ct);
                await fixedObligationStatuses.SaveChangesAsync(ct);
            }

            if (profile.BaselineMonthlyIncome > 0)
            {
                var splits = calc.AllocateIncome(profile.BaselineMonthlyIncome, seededBuckets).ToList();
                var txns = splits.Select(s => Transaction.CreateIncome(
                    UserId, s.BucketId, cycle.Id, s.Amount, "Onboarding", IncomeType.Primary)).ToList();
                await transactions.AddRangeAsync(txns, ct);

                var logPayload = System.Text.Json.JsonSerializer.Serialize(
                    new { Amount = profile.BaselineMonthlyIncome, Source = "Onboarding" });
                await audit.AddAsync(AuditLog.Create(UserId, "ADD_INCOME", "Transaction", null, logPayload), ct);
            }

            await transactions.SaveChangesAsync(ct);
            await audit.SaveChangesAsync(ct);
            await intakes.SaveChangesAsync(ct);
            return Ok(new { message = "Onboarding complete", pathway = dto.Pathway });
        }

        [HttpGet("intake")]
        public async Task<IActionResult> GetIntake(CancellationToken ct)
        {
            var profile = await intakes.GetByUserIdAsync(UserId, ct);
            if (profile is null) return NoContent();

            var activeWants = await wantCategories.GetActiveByUserIdAsync(UserId, ct);
            return Ok(recommender.BuildPathways(profile, activeWants));
        }
    }
}
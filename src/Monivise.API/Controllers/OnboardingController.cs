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
    IAllocationRecommendationService recommender) : ApiControllerBase
    {
        /// <summary>Save intake + baseline, return 3 pathway previews.</summary>
        [HttpPost("intake")]
        public async Task<IActionResult> SubmitIntake([FromBody] SubmitIntakeDto dto, CancellationToken ct)
        {
            decimal committed = dto.Items
                .Where(i => i.Nature != "Unpriced")
                .Sum(i => i.MonthlyAmount);

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
                var staleItems = profile.Items.ToList();          // snapshot before clearing
                await intakes.DeleteItemsByProfileIdAsync(profile.Id, ct);
                intakes.DetachItems(staleItems);                    // stop EF tracking the deleted rows
                profile.ClearItems();
            }

            foreach (var i in dto.Items)
                profile.AddItem(IntakeItem.Create(profile.Id, i.Name,
                    Enum.Parse<ExpenseCategory>(i.Category),
                    Enum.Parse<ItemNature>(i.Nature),
                    i.MonthlyAmount, i.ReserveOnly));

            await intakes.SaveChangesAsync(ct);
            return Ok(recommender.BuildPathways(profile));
        }

        /// <summary>Choose a pathway → seed buckets → record baseline income → complete onboarding.</summary>
        [HttpPost("commit")]
        public async Task<IActionResult> Commit([FromBody] CommitPathwayDto dto, CancellationToken ct)
        {
            var profile = await intakes.GetByUserIdAsync(UserId, ct)
                ?? throw new ArgumentException("No intake profile. Submit intake first.");

            var chosen = Enum.Parse<PathwayType>(dto.Pathway);
            var preview = recommender.BuildPathways(profile).First(p => p.Pathway == dto.Pathway);
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
            await cycles.AddAsync(cycle, ct);

            // Save now so bucket rows have Ids before we split income across them.
            await buckets.SaveChangesAsync(ct);
            await cycles.SaveChangesAsync(ct);

            // Automatically record the income you entered in step 1 — otherwise every
            // bucket sits at ₦0 until you separately re-type the same figure on the
            // Income page, even though it was already used to compute the pathway.
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
            return Ok(recommender.BuildPathways(profile));
        }
    }
}
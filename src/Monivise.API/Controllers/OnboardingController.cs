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
    IAllocationRecommendationService recommender) : ApiControllerBase
    {
        /// <summary>Save intake + baseline, return 3 pathway previews.</summary>
        [HttpPost("intake")]
        public async Task<IActionResult> SubmitIntake([FromBody] SubmitIntakeDto dto, CancellationToken ct)
        {
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

        /// <summary>Choose a pathway → seed buckets → complete onboarding.</summary>
        [HttpPost("commit")]
        public async Task<IActionResult> Commit([FromBody] CommitPathwayDto dto, CancellationToken ct)
        {
            var profile = await intakes.GetByUserIdAsync(UserId, ct)
                ?? throw new ArgumentException("No intake profile. Submit intake first.");

            var chosen = Enum.Parse<PathwayType>(dto.Pathway);
            var preview = recommender.BuildPathways(profile).First(p => p.Pathway == dto.Pathway);
            if (!preview.IsAffordable)
                return UnprocessableEntity(new { code = "PATHWAY_UNAFFORDABLE", preview.AffordabilityGap });

            int order = 0;
            foreach (var b in preview.Buckets)
                await buckets.AddAsync(Bucket.Create(UserId, b.Name, "💰", "#00CFA8",
                    Enum.Parse<BucketType>(b.Type), b.AllocationPercent, order++), ct);

            profile.ChoosePathway(chosen);

            // Create the initial budget cycle for the user
            profile.ChoosePathway(chosen);
            await cycles.AddAsync(BudgetCycle.CreateCurrentMonth(UserId), ct);   
            await buckets.SaveChangesAsync(ct);
            await cycles.SaveChangesAsync(ct);                                  
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

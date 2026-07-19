using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monivise.Application.DTOs.Cycles;
using Monivise.Application.Interfaces.Repositories;
using Monivise.Application.Interfaces.Services;
using Monivise.Domain.Entities;
using Monivise.Domain.Enums;
using Monivise.Domain.Exceptions;
using System.Security.Claims;

namespace Monivise.API.Controllers
{
    [ApiController]
    [Route("api/cycles")]
    [Authorize]
    public class BudgetCyclesController : ControllerBase
    {
        private readonly IBudgetCycleRepository _cycles;
        private readonly IIntakeProfileRepository _intakes;
        private readonly IWantCategoryRepository _wantCategories;
        private readonly IBucketRepository _buckets;
        private readonly ITransactionRepository _transactions;
        private readonly IGoalRepository _goals;
        private readonly IAllocationRecommendationService _recommender;
        private readonly IFinancialCalculationService _calc;
        private readonly ISurplusSweepService _review;
        private readonly IAuditLogRepository _audit;

        private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public BudgetCyclesController(IBudgetCycleRepository cycles, IIntakeProfileRepository intakes,
            IWantCategoryRepository wantCategories, IBucketRepository buckets, ITransactionRepository transactions,
            IGoalRepository goals, IAllocationRecommendationService recommender, IFinancialCalculationService calc,
            ISurplusSweepService review, IAuditLogRepository audit)
        {
            _cycles = cycles; _intakes = intakes; _wantCategories = wantCategories; _buckets = buckets;
            _transactions = transactions; _goals = goals; _recommender = recommender; _calc = calc;
            _review = review; _audit = audit;
        }

        [HttpPost("start")]
        [ProducesResponseType(typeof(CycleResponseDto), 201)]
        public async Task<IActionResult> StartCycle(CancellationToken ct)
        {
            var existing = await _cycles.GetActiveByUserIdAsync(UserId, ct);
            if (existing is not null) throw new DuplicateCycleException();

            var cycle = BudgetCycle.CreateCurrentMonth(UserId);
            await _cycles.AddAsync(cycle, ct);
            await _cycles.SaveChangesAsync(ct);

            return CreatedAtAction(nameof(GetActive), new CycleResponseDto
            {
                Id = cycle.Id,
                StartDate = cycle.StartDate,
                EndDate = cycle.EndDate,
                Status = cycle.Status.ToString()
            });
        }

        [HttpGet("active")]
        [ProducesResponseType(typeof(CycleResponseDto), 200)]
        public async Task<IActionResult> GetActive(CancellationToken ct)
        {
            var cycle = await _cycles.GetActiveByUserIdAsync(UserId, ct);
            if (cycle is null) return NotFound();

            return Ok(new CycleResponseDto
            {
                Id = cycle.Id,
                StartDate = cycle.StartDate,
                EndDate = cycle.EndDate,
                Status = cycle.Status.ToString()
            });
        }

        /// <summary>
        /// Closes the current cycle and opens the next one. Only available once the
        /// current cycle is in its final week — "the option to record that salary was
        /// paid" only unlocks near month-end, matching how pay dates actually work.
        /// If there's leftover surplus, the caller must say where it goes (same
        /// Buffer/Wants/Goal split as Extra income) before the rollover proceeds.
        /// </summary>
        [HttpPost("rollover")]
        public async Task<IActionResult> Rollover([FromBody] RolloverRequestDto dto, CancellationToken ct)
        {
            var oldCycle = await _cycles.GetActiveByUserIdAsync(UserId, ct)
                ?? throw new CycleNotFoundException(UserId);

            if (oldCycle.RemainingDays > 7)
                return Conflict(new { code = "CYCLE_NOT_YET_ELIGIBLE", detail = "Rollover unlocks once you're in the final week of your current cycle." });

            var profile = await _intakes.GetByUserIdAsync(UserId, ct)
                ?? throw new ArgumentException("No intake profile.");
            if (profile.ChosenPathway is null)
                throw new ArgumentException("No pathway on file — cannot reseed the next cycle.");

            // ── Step 1: settle the outgoing cycle's surplus ──
            var review = await _review.BuildReviewAsync(UserId, ct);

            if (review.TotalSurplus > 0)
            {
                if (dto.Splits is null || dto.Splits.Count == 0)
                    return BadRequest(new { code = "SPLITS_REQUIRED", review.TotalSurplus, detail = "This cycle has a surplus — say where it should go before rolling over." });

                var validDestinations = new[] { "Buffer", "Wants", "Goal" };
                if (dto.Splits.Any(s => !validDestinations.Contains(s.Destination)))
                    return BadRequest(new { code = "INVALID_DESTINATION" });

                decimal totalPct = dto.Splits.Sum(s => s.Percent);
                if (Math.Abs(totalPct - 100m) > 0.01m)
                    return BadRequest(new { code = "SPLITS_MUST_SUM_TO_100", totalPct });

                Goal? activeGoal = null;
                if (dto.Splits.Any(s => s.Destination == "Goal"))
                {
                    activeGoal = await _goals.GetActiveAsync(UserId, ct);
                    if (activeGoal is null)
                        return UnprocessableEntity(new { code = "NO_ACTIVE_GOAL" });
                }

                decimal remainder = review.TotalSurplus;
                var applied = new List<(string Destination, decimal Amount)>();
                for (int i = 0; i < dto.Splits.Count; i++)
                {
                    bool isLast = i == dto.Splits.Count - 1;
                    decimal share = isLast ? remainder : Math.Round(review.TotalSurplus * dto.Splits[i].Percent / 100m, 2);
                    remainder -= share;
                    if (share <= 0) continue;

                    switch (dto.Splits[i].Destination)
                    {
                        case "Buffer": oldCycle.AddToBuffer(share); break;
                        case "Wants": oldCycle.AddToUnpricedPool(share); break;
                        case "Goal": activeGoal!.Contribute(share); break;
                    }
                    applied.Add((dto.Splits[i].Destination, share));
                }

                if (activeGoal is not null) await _goals.SaveChangesAsync(ct);

                var logPayload = System.Text.Json.JsonSerializer.Serialize(new
                {
                    review.TotalSurplus,
                    Splits = applied.Select(a => new { a.Destination, a.Amount })
                });
                await _audit.AddAsync(AuditLog.Create(UserId, "CYCLE_ROLLOVER_SURPLUS", "BudgetCycle", oldCycle.Id, logPayload), ct);
            }

            // ── Step 2: close the old cycle, open the new one ──
            oldCycle.Close();
            await _cycles.SaveChangesAsync(ct);

            var newCycle = BudgetCycle.CreateCurrentMonth(UserId);
            await _cycles.AddAsync(newCycle, ct);
            await _cycles.SaveChangesAsync(ct);

            // ── Step 3: reseed Buffer/Pool from the same pathway, no re-choosing ──
            var activeWants = await _wantCategories.GetActiveByUserIdAsync(UserId, ct);
            var preview = _recommender.BuildPathways(profile, activeWants)
                .First(p => p.Pathway == profile.ChosenPathway.ToString());

            newCycle.SeedBuffer(preview.BufferAmount);
            newCycle.SeedUnpricedPool(preview.UnpricedWantsPoolAmount);
            await _cycles.SaveChangesAsync(ct);

            // ── Step 4: auto-record Primary income for the new cycle ──
            var buckets = (await _buckets.GetActiveByUserIdAsync(UserId, ct)).ToList();
            if (profile.BaselineMonthlyIncome > 0)
            {
                var splits = _calc.AllocateIncome(profile.BaselineMonthlyIncome, buckets).ToList();
                var txns = splits.Select(s => Transaction.CreateIncome(
                    UserId, s.BucketId, newCycle.Id, s.Amount, "Salary", IncomeType.Primary)).ToList();
                await _transactions.AddRangeAsync(txns, ct);
                await _transactions.SaveChangesAsync(ct);
            }

            await _audit.SaveChangesAsync(ct);

            return Ok(new
            {
                message = "Cycle rolled over",
                previousCycleId = oldCycle.Id,
                newCycleId = newCycle.Id,
                surplusSwept = review.TotalSurplus
            });
        }
    }
}
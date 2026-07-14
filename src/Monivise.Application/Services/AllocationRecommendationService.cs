using Monivise.Application.DTOs.Onboarding;
using Monivise.Application.Interfaces.Services;
using Monivise.Domain.Entities;
using Monivise.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Monivise.Application.Services
{
    public class AllocationRecommendationService : IAllocationRecommendationService
    {
        // ── TUNABLE CONSTANTS ──
        private const decimal DaysInMonth = 30m;

        // Pathways no longer tilt Fixed Obligations or Savings — those are protected
        // and identical across all three. They only decide how *leftover* money,
        // after Fixed + Savings + Flexible are covered, splits between an extra
        // top-up to Savings and genuinely discretionary Wants spending.
        private static (decimal extraSavingsShare, decimal wantsShare) ResidualSplit(PathwayType p) => p switch
        {
            PathwayType.Prudent => (0.70m, 0.30m),      // mostly tops up savings further
            PathwayType.Moderate => (0.50m, 0.50m),     // even split
            PathwayType.Comfortable => (0.15m, 0.85m),  // mostly goes to Wants
            _ => (0.50m, 0.50m)
        };

        public IEnumerable<PathwayPreviewDto> BuildPathways(IntakeProfile profile)
        {
            var items = profile.Items.ToList();
            decimal income = profile.BaselineMonthlyIncome;

            // Fixed Obligations: Need + HardFixed, never Investment-tagged (avoids double-counting
            // an item like Pension, which is Investment + HardFixed).
            decimal hard = items
                .Where(i => i.Nature == ItemNature.HardFixed && i.Category != ExpenseCategory.Investment)
                .Sum(i => i.MonthlyAmount);

            // Savings: everything tagged Investment, regardless of its Nature. This is the
            // protected baseline — identical across all three pathways, no tilt applied.
            decimal savingsBase = items
                .Where(i => i.Category == ExpenseCategory.Investment)
                .Sum(i => i.MonthlyAmount);

            // Flexible Spending: Need/Want + Soft, not reserve-only, not Investment-tagged.
            // Semi-protected — normally left alone, but can be clipped if income is tight.
            decimal softBase = items
                .Where(i => i.Nature == ItemNature.Soft && !i.ReserveOnly && i.Category != ExpenseCategory.Investment)
                .Sum(i => i.MonthlyAmount);

            foreach (PathwayType p in Enum.GetValues<PathwayType>())
                yield return Build(p, income, hard, savingsBase, softBase, items);
        }

        private static PathwayPreviewDto Build(PathwayType p, decimal income,
            decimal hard, decimal savingsBase, decimal softBase, List<IntakeItem> items)
        {
            var dto = new PathwayPreviewDto { Pathway = p.ToString() };

            // Step 1 — the only affordability question that matters: can income even
            // cover the two truly protected categories? This is now identical for
            // every pathway, so it's impossible for one to be affordable and another not.
            decimal coreProtected = hard + savingsBase;
            if (coreProtected > income)
            {
                dto.IsAffordable = false;
                dto.AffordabilityGap = Math.Round(coreProtected - income, 2);
                dto.SuggestedCuts = items
                    .Where(i => i.Category == ExpenseCategory.Investment)
                    .OrderByDescending(i => i.MonthlyAmount)
                    .Take(3)
                    .Select(i => $"Reduce {i.Name} (₦{i.MonthlyAmount:N0}/mo)")
                    .ToList();
                return dto;
            }

            // Step 2 — Flexible Spending is protected next, but clipped if the money
            // genuinely isn't there ("shouldn't be deducted from, but if need be it could").
            decimal remainingAfterCore = income - coreProtected;
            decimal flexActual = Math.Min(softBase, remainingAfterCore);
            decimal flexShortfall = Math.Max(0, softBase - remainingAfterCore);

            // Step 3 — whatever's left after protecting Fixed + Savings + Flexible is the
            // residual pool. This is the ONLY thing pathways disagree about.
            decimal residual = Math.Max(0, remainingAfterCore - flexActual);
            var (extraSavingsShare, wantsShare) = ResidualSplit(p);
            decimal extraSavings = Math.Round(residual * extraSavingsShare, 2);
            decimal wants = Math.Round(residual - extraSavings, 2); // remainder avoids rounding drift

            decimal totalSavings = savingsBase + extraSavings;

            dto.IsAffordable = true;
            dto.MonthlySavings = Math.Round(totalSavings, 2);
            dto.SaveRate = income > 0 ? Math.Round(totalSavings / income * 100m, 1) : 0;

            if (flexShortfall > 0)
                dto.SuggestedCuts = items
                    .Where(i => i.Nature == ItemNature.Soft && !i.ReserveOnly)
                    .OrderByDescending(i => i.MonthlyAmount)
                    .Take(3)
                    .Select(i => $"Trim {i.Name} (₦{i.MonthlyAmount:N0}/mo) — flexible spending is ₦{flexShortfall:N0} short this pathway")
                    .ToList();

            decimal safeToSpend = flexActual + wants; // the genuinely spendable pool
            dto.DailyLimit = Math.Round(safeToSpend / DaysInMonth, 2);
            dto.WeeklyLimit = Math.Round(dto.DailyLimit * 7m, 2);

            // Bucket percentages — by construction these sum to exactly `income`,
            // so percentages are computed straight against income, no normalization needed.
            decimal denom = income > 0 ? income : 1m;
            dto.Buckets =
            [
                new() { Name = "Fixed Obligations", Type = "Fixed",    AllocationPercent = Pct(hard, denom) },
                new() { Name = "Flexible Spending",  Type = "Flexible", AllocationPercent = Pct(flexActual, denom) },
                new() { Name = "Wants",              Type = "Flexible", AllocationPercent = Pct(wants, denom) },
                new() { Name = "Savings",            Type = "Savings",  AllocationPercent = Pct(totalSavings, denom) },
            ];
            // last bucket absorbs rounding remainder so the four sum to exactly 100
            decimal sum = dto.Buckets.Take(3).Sum(b => b.AllocationPercent);
            dto.Buckets[^1].AllocationPercent = Math.Round(100m - sum, 2);

            return dto;
        }

        private static decimal Pct(decimal part, decimal total) => Math.Round(part / total * 100m, 2);
    }
}
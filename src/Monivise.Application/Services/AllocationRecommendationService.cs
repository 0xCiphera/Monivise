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
        private const decimal DaysInMonth = 30m;

        // The ONLY thing pathways disagree about now: how the residual — whatever's
        // left after Fixed + Investment + Flexible + priced Wants — splits between
        // Buffer (safety margin) and the unpriced-Wants pool.
        private static (decimal bufferShare, decimal unpricedShare) ResidualSplit(PathwayType p) => p switch
        {
            PathwayType.Prudent => (0.70m, 0.30m),
            PathwayType.Moderate => (0.50m, 0.50m),
            PathwayType.Comfortable => (0.25m, 0.75m),
            _ => (0.50m, 0.50m)
        };

        public IEnumerable<PathwayPreviewDto> BuildPathways(IntakeProfile profile, IEnumerable<WantCategory> wantCategories)
        {
            var items = profile.Items.ToList();
            var wants = wantCategories.ToList();
            decimal income = profile.BaselineMonthlyIncome;

            decimal hard = items
                .Where(i => i.Nature == ItemNature.HardFixed && i.Category != ExpenseCategory.Investment)
                .Sum(i => i.MonthlyAmount);

            decimal investmentBase = items
                .Where(i => i.Category == ExpenseCategory.Investment)
                .Sum(i => i.MonthlyAmount);

            decimal flexBase = items
                .Where(i => i.Nature == ItemNature.Soft && !i.ReserveOnly && i.Category != ExpenseCategory.Investment)
                .Sum(i => i.MonthlyAmount);

            decimal wantsPricedTotal = wants.Where(w => !w.IsUnpriced).Sum(w => w.MonthlyAmount);
            int unpricedCount = wants.Count(w => w.IsUnpriced);

            foreach (PathwayType p in Enum.GetValues<PathwayType>())
                yield return Build(p, income, hard, investmentBase, flexBase, wantsPricedTotal, unpricedCount, items);
        }

        private static PathwayPreviewDto Build(PathwayType p, decimal income,
            decimal hard, decimal investmentBase, decimal flexBase, decimal wantsPricedTotal,
            int unpricedCount, List<IntakeItem> items)
        {
            var dto = new PathwayPreviewDto { Pathway = p.ToString() };

            // Affordability gate — unchanged in spirit from before: can income even cover
            // the two truly non-negotiable categories? Identical across all 3 pathways.
            decimal coreProtected = hard + investmentBase;
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

            decimal remaining = income - coreProtected;

            // Flexible Spending — protected next, clipped if the money genuinely isn't there.
            decimal flexActual = Math.Min(flexBase, remaining);
            decimal flexShortfall = Math.Max(0, flexBase - remaining);
            remaining -= flexActual;

            // Wants (priced) — lower priority than Flexible, same clipping treatment.
            decimal wantsPricedActual = Math.Min(wantsPricedTotal, remaining);
            remaining -= wantsPricedActual;

            // Residual — the only pathway-dependent split.
            var (bufferShare, unpricedShare) = ResidualSplit(p);
            decimal bufferAmount = Math.Round(remaining * bufferShare, 2);
            decimal unpricedPoolAmount = Math.Round(remaining - bufferAmount, 2); // remainder avoids rounding drift

            dto.IsAffordable = true;
            dto.MonthlySavings = Math.Round(investmentBase, 2);
            dto.SaveRate = income > 0 ? Math.Round(investmentBase / income * 100m, 1) : 0;
            dto.BufferAmount = bufferAmount;
            dto.UnpricedWantsPoolAmount = unpricedPoolAmount;

            if (flexShortfall > 0)
                dto.SuggestedCuts = items
                    .Where(i => i.Nature == ItemNature.Soft && !i.ReserveOnly)
                    .OrderByDescending(i => i.MonthlyAmount)
                    .Take(3)
                    .Select(i => $"Trim {i.Name} (₦{i.MonthlyAmount:N0}/mo) — flexible spending is ₦{flexShortfall:N0} short")
                    .ToList();

            decimal safeToSpend = flexActual + wantsPricedActual;
            dto.DailyLimit = Math.Round(safeToSpend / DaysInMonth, 2);
            dto.WeeklyLimit = Math.Round(dto.DailyLimit * 7m, 2);

            decimal wantsTotal = wantsPricedActual + unpricedPoolAmount;
            decimal denom = income > 0 ? income : 1m;
            dto.Buckets =
            [
                new() { Name = "Fixed Obligations", Type = "Fixed",      AllocationPercent = Pct(hard, denom) },
                new() { Name = "Flexible Spending",  Type = "Flexible",  AllocationPercent = Pct(flexActual, denom) },
                new() { Name = "Investment",         Type = "Investment", AllocationPercent = Pct(investmentBase, denom) },
                new() { Name = "Wants",              Type = "Wants",     AllocationPercent = Pct(wantsTotal, denom) },
            ];
            decimal sum = dto.Buckets.Take(3).Sum(b => b.AllocationPercent);
            dto.Buckets[^1].AllocationPercent = Math.Round(100m - sum, 2);

            return dto;
        }

        private static decimal Pct(decimal part, decimal total) => Math.Round(part / total * 100m, 2);
    }
}
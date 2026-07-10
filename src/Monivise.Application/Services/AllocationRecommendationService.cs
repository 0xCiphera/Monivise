using Monivise.Application.DTOs.Onboarding;
using Monivise.Application.Interfaces.Services;
using Monivise.Domain.Entities;
using Monivise.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Application.Services
{
    public class AllocationRecommendationService : IAllocationRecommendationService
    {
        // ── TUNABLE CONSTANTS ──
        private const decimal DaysInMonth = 30m;
        // pathway → (softFactor, investTilt, wantsTilt)
        private static (decimal soft, decimal invest, decimal wants) Tilt(PathwayType p) => p switch
        {
            PathwayType.Prudent => (0.65m, 1.50m, 0.60m),
            PathwayType.Moderate => (1.00m, 1.00m, 1.00m),
            PathwayType.Comfortable => (1.15m, 0.70m, 1.30m),
            _ => (1.00m, 1.00m, 1.00m)
        };

        public IEnumerable<PathwayPreviewDto> BuildPathways(IntakeProfile profile)
        {
            var items = profile.Items.ToList();
            decimal income = profile.BaselineMonthlyIncome;

            decimal hard = items.Where(i => i.Nature == ItemNature.HardFixed && i.Category != ExpenseCategory.Investment)
                                .Sum(i => i.MonthlyAmount);
            decimal softBase = items.Where(i => i.Nature == ItemNature.Soft && !i.ReserveOnly && i.Category != ExpenseCategory.Investment)
                                    .Sum(i => i.MonthlyAmount);
            decimal investBase = items.Where(i => i.Category == ExpenseCategory.Investment)
                                      .Sum(i => i.MonthlyAmount);

            foreach (PathwayType p in Enum.GetValues<PathwayType>())
                yield return Build(p, income, hard, softBase, investBase, items);
        }

        private static PathwayPreviewDto Build(PathwayType p, decimal income,
            decimal hard, decimal softBase, decimal investBase, List<IntakeItem> items)
        {
            var (softF, investT, wantsT) = Tilt(p);

            decimal soft = Math.Round(softBase * softF, 2);
            decimal invest = Math.Round(investBase * investT, 2);
            decimal committed = hard + soft + invest;

            var dto = new PathwayPreviewDto { Pathway = p.ToString() };

            if (committed > income)
            {
                dto.IsAffordable = false;
                dto.AffordabilityGap = Math.Round(committed - income, 2);
                dto.SuggestedCuts = items
                    .Where(i => i.Nature == ItemNature.Soft && !i.ReserveOnly)
                    .OrderByDescending(i => i.MonthlyAmount)
                    .Take(3)
                    .Select(i => $"Trim {i.Name} (₦{i.MonthlyAmount:N0}/mo)")
                    .ToList();
                return dto;
            }

            decimal wantsRaw = income - committed;
            decimal wants = Math.Round(wantsRaw * Math.Min(1m, wantsT), 2);
            decimal savings = invest + (wantsRaw - wants); // Prudent diverts leftover to savings

            dto.IsAffordable = true;
            dto.MonthlySavings = Math.Round(savings, 2);
            dto.SaveRate = income > 0 ? Math.Round(savings / income * 100m, 1) : 0;

            decimal safeToSpend = soft + wants;             // flexible spendable
            dto.DailyLimit = Math.Round(safeToSpend / DaysInMonth, 2);
            dto.WeeklyLimit = Math.Round(dto.DailyLimit * 7m, 2);

            // bucket % — normalize hard/soft/wants/savings to 100
            decimal total = hard + soft + wants + savings;
            if (total <= 0) total = 1m;
            dto.Buckets =
            [
                new() { Name = "Fixed Obligations", Type = "Fixed",
                    AllocationPercent = Pct(hard, total) },
            new() { Name = "Flexible Spending", Type = "Flexible",
                    AllocationPercent = Pct(soft, total) },
            new() { Name = "Wants",             Type = "Flexible",
                    AllocationPercent = Pct(wants, total) },
            new() { Name = "Savings",           Type = "Savings",
                    AllocationPercent = Pct(savings, total) },
        ];
            // last bucket absorbs rounding remainder → sum to exactly 100
            decimal sum = dto.Buckets.Take(3).Sum(b => b.AllocationPercent);
            dto.Buckets[^1].AllocationPercent = Math.Round(100m - sum, 2);

            return dto;
        }

        private static decimal Pct(decimal part, decimal total) => Math.Round(part / total * 100m, 2);
    }
}
using Monivise.Application.DTOs.Review;
using Monivise.Application.Interfaces.Repositories;
using Monivise.Application.Interfaces.Services;
using Monivise.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Application.Services
{
    public class SurplusSweepService(
     IBudgetCycleRepository cycles,
     IBucketRepository buckets,
     IIntakeProfileRepository intakes,
     IGoalRepository goals,
     IFinancialCalculationService finance) : ISurplusSweepService
    {
        public async Task<WeeklyReviewDto> BuildReviewAsync(Guid userId, CancellationToken ct = default)
        {
            var cycle = await cycles.GetActiveByUserIdAsync(userId, ct);
            var profile = await intakes.GetByUserIdAsync(userId, ct);
            var active = await goals.GetActiveAsync(userId, ct);

            var dto = new WeeklyReviewDto();

            // Fixed prompts come from intake HardFixed items (weekly portion = monthly / 4)
            if (profile != null)
                dto.FixedPrompts = profile.Items
                    .Where(i => i.Nature == ItemNature.HardFixed)
                    .Select(i => new FixedActualDto
                    {
                        IntakeItemId = i.Id,
                        Name = i.Name,
                        Reserved = Math.Round(i.MonthlyAmount / 4m, 2),
                        ActualSpent = 0
                    }).ToList();

            // Daily underspend over the past 7 days (computed; surplus filled on apply)
            if (cycle != null)
            {
                var bks = await buckets.GetActiveByUserIdAsync(userId, ct);
                decimal safe = finance.GetSafeToSpend(bks, cycle.Transactions);
                decimal pace = finance.GetSpendingPace(bks, cycle.Transactions, cycle);
                decimal daily = finance.GetDailyLimit(safe, pace, cycle);

                var weekStart = DateTime.UtcNow.Date.AddDays(-7);
                var dailySpend = cycle.Transactions
                    .Where(t => t.Kind == TransactionKind.Expense && t.Date.Date >= weekStart)
                    .GroupBy(t => t.Date.Date)
                    .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

                var daysElapsed = Math.Min(7, (DateTime.UtcNow.Date - weekStart).Days);
                decimal underspend = 0m;
                for (int i = 0; i < daysElapsed; i++)
                {
                    var day = weekStart.AddDays(i);
                    var spentThatDay = dailySpend.TryGetValue(day, out var v) ? v : 0m;
                    underspend += Math.Max(0, daily - spentThatDay);
                }
                dto.DailyUnderspend = Math.Round(underspend, 2);
            }

            if (active != null)
                dto.ActiveGoal = new GoalRef
                {
                    Id = active.Id,
                    Name = active.Name,
                    ProgressPercent = active.ProgressPercent
                };

            dto.TotalSurplus = dto.FixedPrompts.Sum(f => Math.Max(0, f.Reserved - f.ActualSpent))
                               + dto.DailyUnderspend;
            return dto;
        }

        public async Task ApplySweepAsync(Guid userId, ApplySweepDto dto, CancellationToken ct = default)
        {
            var goal = await goals.GetByIdAsync(dto.GoalId, ct)
                       ?? throw new ArgumentException("Goal not found");
            if (goal.UserId != userId) throw new UnauthorizedAccessException();
            if (dto.Amount > 0) goal.Contribute(dto.Amount);
            await goals.SaveChangesAsync(ct);
        }
    }
}

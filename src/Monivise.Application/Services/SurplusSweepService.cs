using Monivise.Application.DTOs.Review;
using Monivise.Application.Interfaces.Repositories;
using Monivise.Application.Interfaces.Services;
using Monivise.Domain.Entities;
using Monivise.Domain.Enums;
using Monivise.Domain.Exceptions;
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
     IWantCategoryRepository wantCategories,
     IFixedObligationStatusRepository fixedObligationStatuses,
     ITransactionRepository transactions) : ISurplusSweepService
    {
        public async Task<WeeklyReviewDto> BuildReviewAsync(Guid userId, CancellationToken ct = default)
        {
            var cycle = await cycles.GetActiveByUserIdAsync(userId, ct)
                ?? throw new CycleNotFoundException(userId);
            var profile = await intakes.GetByUserIdAsync(userId, ct);
            var wants = (await wantCategories.GetActiveByUserIdAsync(userId, ct)).ToList();
            var active = await goals.GetActiveAsync(userId, ct);
            var txns = cycle.Transactions.ToList();

            var dto = new WeeklyReviewDto
            {
                IsMonthEnd = cycle.RemainingDays <= 7,
                UnpricedPoolBalance = cycle.UnpricedWantsPoolBalance,
                BufferBalance = cycle.BufferBalance
            };

            var weekStart = DateTime.UtcNow.Date.AddDays(-7);

            if (profile != null)
            {
                // Flexible — this week's slice only.
                dto.Flexible = profile.Items
                    .Where(i => i.Nature == ItemNature.Soft && !i.ReserveOnly && i.Category != ExpenseCategory.Investment)
                    .Select(i => new ItemActualDto
                    {
                        IntakeItemId = i.Id,
                        Name = i.Name,
                        Reserved = Math.Round(i.MonthlyAmount * 7m / 30m, 2),
                        Actual = txns.Where(t => t.Kind == TransactionKind.Expense
                                && t.IntakeItemId == i.Id && t.Date.Date >= weekStart)
                            .Sum(t => t.Amount)
                    }).ToList();

                // Investment — cumulative this cycle, not just this week.
                dto.Investment = profile.Items
                    .Where(i => i.Category == ExpenseCategory.Investment)
                    .Select(i => new ItemActualDto
                    {
                        IntakeItemId = i.Id,
                        Name = i.Name,
                        Reserved = i.MonthlyAmount,
                        Actual = txns.Where(t => t.Kind == TransactionKind.Income && t.IntakeItemId == i.Id)
                            .Sum(t => t.Amount)
                    }).ToList();
            }

            // Wants (priced) — this week's slice, same convention as Flexible.
            dto.WantsPriced = wants.Where(w => !w.IsUnpriced)
                .Select(w => new ItemActualDto
                {
                    WantCategoryId = w.Id,
                    Name = w.Name,
                    Reserved = Math.Round(w.MonthlyAmount * 7m / 30m, 2),
                    Actual = txns.Where(t => t.Kind == TransactionKind.Expense
                            && t.WantCategoryId == w.Id && t.Date.Date >= weekStart)
                        .Sum(t => t.Amount)
                }).ToList();

            if (dto.IsMonthEnd)
            {
                var statuses = await fixedObligationStatuses.GetByCycleIdAsync(cycle.Id, ct);
                // Surplus only ever comes from an obligation being skipped entirely this
                // cycle (Actual = 0, so the full Reserved amount frees up) — a paid item
                // is always paid at exactly its reserved amount, contributing nothing.
                dto.FixedObligations = statuses.Where(s => !s.IsPaid)
                    .Select(s => new ItemActualDto
                    {
                        IntakeItemId = s.IntakeItemId,
                        Name = s.Item.Name,
                        Reserved = s.Item.MonthlyAmount,
                        Actual = 0
                    }).ToList();
            }

            if (active != null)
                dto.ActiveGoal = new GoalRef { Id = active.Id, Name = active.Name, ProgressPercent = active.ProgressPercent };

            dto.TotalSurplus = ComputeSurplus(dto);
            return dto;
        }

        private static decimal ComputeSurplus(WeeklyReviewDto dto)
        {
            decimal flexSurplus = dto.Flexible.Sum(i => Math.Max(0, i.Reserved - i.Actual));
            decimal investShortfall = dto.Investment.Sum(i => Math.Max(0, i.Reserved - i.Actual));
            decimal wantsSurplus = dto.WantsPriced.Sum(i => Math.Max(0, i.Reserved - i.Actual));
            decimal total = flexSurplus + investShortfall + wantsSurplus;
            if (dto.IsMonthEnd)
                total += dto.FixedObligations.Sum(i => Math.Max(0, i.Reserved - i.Actual));
            return Math.Round(total, 2);
        }
        public async Task ApplySweepAsync(Guid userId, ApplySweepDto dto, CancellationToken ct = default)
        {
            var cycle = await cycles.GetActiveByUserIdAsync(userId, ct)
                ?? throw new CycleNotFoundException(userId);
            var review = await BuildReviewAsync(userId, ct);

            if (review.TotalSurplus <= 0) return; // nothing to sweep, no-op

            if (dto.Splits is null || dto.Splits.Count == 0)
                throw new ValidationException("SPLITS_REQUIRED", "This review has a surplus — say where it should go before sweeping.");

            var validDestinations = new[] { "Buffer", "Wants", "Goal" };
            if (dto.Splits.Any(s => !validDestinations.Contains(s.Destination)))
                throw new ValidationException("INVALID_DESTINATION", "Destination must be Buffer, Wants, or Goal.");

            decimal totalPct = dto.Splits.Sum(s => s.Percent);
            if (Math.Abs(totalPct - 100m) > 0.01m)
                throw new ValidationException("SPLITS_MUST_SUM_TO_100", $"Splits must sum to 100 (got {totalPct}).");

            Goal? activeGoal = null;
            if (dto.Splits.Any(s => s.Destination == "Goal"))
            {
                activeGoal = await goals.GetActiveAsync(userId, ct);
                if (activeGoal is null) throw new NoActiveGoalException();
            }

            var userBuckets = (await buckets.GetActiveByUserIdAsync(userId, ct)).ToList();
            Guid flexibleBucketId = userBuckets.First(b => b.Type == BucketType.Flexible).Id;
            Guid wantsBucketId = userBuckets.First(b => b.Type == BucketType.Wants).Id;
            Guid investmentBucketId = userBuckets.First(b => b.Type == BucketType.Investment).Id;
            Guid fixedBucketId = userBuckets.First(b => b.Type == BucketType.Fixed).Id;

            // Close out each contributing item — unchanged from before, still happens
            // regardless of where the surplus itself ends up.
            foreach (var item in review.Flexible.Where(i => i.Reserved > i.Actual))
                await transactions.AddAsync(Transaction.CreateExpense(userId, flexibleBucketId, cycle.Id,
                    item.Reserved - item.Actual, "Swept at review", item.IntakeItemId), ct);

            foreach (var item in review.WantsPriced.Where(i => i.Reserved > i.Actual))
                await transactions.AddAsync(Transaction.CreateExpense(userId, wantsBucketId, cycle.Id,
                    item.Reserved - item.Actual, "Swept at review", null, item.WantCategoryId), ct);

            foreach (var item in review.Investment.Where(i => i.Reserved > i.Actual))
                await transactions.AddAsync(Transaction.CreateIncome(userId, investmentBucketId, cycle.Id,
                    item.Reserved - item.Actual, "Swept shortfall at review", IncomeType.Primary, item.IntakeItemId), ct);

            if (review.IsMonthEnd)
                foreach (var item in review.FixedObligations.Where(i => i.Reserved > i.Actual))
                    await transactions.AddAsync(Transaction.CreateExpense(userId, fixedBucketId, cycle.Id,
                        item.Reserved - item.Actual, "Swept at review", item.IntakeItemId), ct);

            // Apply the surplus itself across whatever destinations were chosen.
            decimal remainder = review.TotalSurplus;
            for (int i = 0; i < dto.Splits.Count; i++)
            {
                bool isLast = i == dto.Splits.Count - 1;
                decimal share = isLast ? remainder : Math.Round(review.TotalSurplus * dto.Splits[i].Percent / 100m, 2);
                remainder -= share;
                if (share <= 0) continue;

                switch (dto.Splits[i].Destination)
                {
                    case "Buffer": cycle.AddToBuffer(share); break;
                    case "Wants": cycle.AddToUnpricedPool(share); break;
                    case "Goal": activeGoal!.Contribute(share); break;
                }
            }

            await transactions.SaveChangesAsync(ct);
            await cycles.SaveChangesAsync(ct);
            if (activeGoal is not null) await goals.SaveChangesAsync(ct);
        }
    }
}
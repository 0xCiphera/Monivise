using Monivise.Application.DTOs.Dashboard;
using Monivise.Application.Interfaces.Services;
using Monivise.Domain.Entities;
using Monivise.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Application.Services
{
    public class FinancialCalculationService : IFinancialCalculationService
    {
        // ─── CORE BUCKET METRICS ───

        public decimal GetAllocated(Guid bucketId, IEnumerable<Transaction> txns)
            => txns.Where(t => t.BucketId == bucketId && t.Kind == TransactionKind.Income).Sum(t => t.Amount);

        public decimal GetSpent(Guid bucketId, IEnumerable<Transaction> txns)
            => txns.Where(t => t.BucketId == bucketId && t.Kind == TransactionKind.Expense).Sum(t => t.Amount);

        public decimal GetBalance(Guid bucketId, IEnumerable<Transaction> txns)
            => GetAllocated(bucketId, txns) - GetSpent(bucketId, txns);

        // ─── SAFE TO SPEND ───

        public decimal GetSafeToSpend(IEnumerable<Bucket> buckets, IEnumerable<Transaction> txns)
        {
            var txnList = txns.ToList();
            var bucketList = buckets.ToList();

            var flexBuckets = bucketList.Where(b => b.Type == BucketType.Flexible && b.IsActive);
            var fixedBuckets = bucketList.Where(b => b.Type == BucketType.Fixed && b.IsActive);

            decimal flexibleRemaining = flexBuckets.Sum(b => GetBalance(b.Id, txnList));
            decimal unpaidFixedObligations = fixedBuckets.Sum(b => Math.Max(0m, GetBalance(b.Id, txnList)));

            return Math.Max(0m, flexibleRemaining - unpaidFixedObligations);
        }

        // ─── SPENDING PACE ───

        public decimal GetSpendingPace(IEnumerable<Bucket> buckets, IEnumerable<Transaction> txns, BudgetCycle cycle)
        {
            var txnList = txns.ToList();
            var flexBuckets = buckets.Where(b => b.Type == BucketType.Flexible && b.IsActive).ToList();

            decimal totalAllocated = flexBuckets.Sum(b => GetAllocated(b.Id, txnList));
            decimal totalSpent = flexBuckets.Sum(b => GetSpent(b.Id, txnList));

            if (totalAllocated == 0) return 1m;

            decimal expectedByToday = (totalAllocated / cycle.TotalDays) * cycle.ElapsedDays;
            if (expectedByToday == 0) return 1m;

            return totalSpent / expectedByToday;
        }

        // ─── DAILY LIMIT ───

        public decimal GetDailyLimit(decimal safeToSpend, decimal pace, BudgetCycle cycle)
        {
            int daysRemaining = cycle.RemainingDays;
            if (daysRemaining <= 0) return 0m;

            decimal paceAdjustment = pace switch
            {
                <= 0.80m => 1.15m,
                <= 1.00m => 1.00m,
                <= 1.20m => 0.85m,
                _ => 0.70m
            };

            return Math.Max(0m, (safeToSpend / daysRemaining) * paceAdjustment);
        }

        // ─── INCOME ALLOCATION ───

        public IEnumerable<AllocationPreview> AllocateIncome(decimal amount, IEnumerable<Bucket> activeBuckets)
        {
            var buckets = activeBuckets.ToList();
            decimal totalPct = buckets.Sum(b => b.AllocationPercent);
            if (totalPct == 0 || buckets.Count == 0) return [];

            var sorted = buckets.OrderByDescending(b => b.AllocationPercent).ToList();
            var result = new List<AllocationPreview>();
            decimal remainder = amount;

            for (int i = 0; i < sorted.Count; i++)
            {
                var bucket = sorted[i];
                bool isLast = i == sorted.Count - 1;
                decimal allocated = isLast
                    ? remainder
                    : Math.Round(amount * (bucket.AllocationPercent / totalPct), 2);
                remainder -= allocated;

                result.Add(new AllocationPreview
                {
                    BucketId = bucket.Id,
                    BucketName = bucket.Name,
                    Amount = allocated
                });
            }

            return result;
        }

        // ─── RISK LEVEL ───

        public RiskLevel GetRiskLevel(decimal postSafeToSpend, decimal postDailyLimit,
            decimal avgDailyBudget, decimal depletionPercent)
        {
            if (postSafeToSpend < 0 || depletionPercent > 100) return RiskLevel.Critical;
            if (postDailyLimit < avgDailyBudget * 0.5m || depletionPercent > 85) return RiskLevel.Risky;
            if (postDailyLimit < avgDailyBudget * 0.8m || depletionPercent > 70) return RiskLevel.Caution;
            return RiskLevel.Safe;
        }

        // ─── DECISION SIMULATION ───

        public DecisionSimulationResult SimulateDecision(Guid bucketId, Guid? intakeItemId, Guid? wantCategoryId,
    decimal amount, IEnumerable<Bucket> buckets, IEnumerable<Transaction> txns, BudgetCycle cycle)
        {
            var txnList = txns.ToList();
            var bucketList = buckets.ToList();

            var bucket = bucketList.FirstOrDefault(b => b.Id == bucketId)
                ?? throw new ArgumentException($"Bucket {bucketId} not found.");

            decimal safeToSpendBefore = GetSafeToSpend(bucketList, txnList);
            decimal pace = GetSpendingPace(bucketList, txnList, cycle);
            decimal dailyLimitBefore = GetDailyLimit(safeToSpendBefore, pace, cycle);
            decimal bucketBalanceBefore = GetBalance(bucketId, txnList);

            // ── Item-level check, when the caller targets a specific item ──
            bool willDrawFromBuffer = false;
            decimal bufferDrawAmount = 0m;
            decimal bufferBalanceAfter = cycle.BufferBalance;

            if (intakeItemId is not null || wantCategoryId is not null)
            {
                decimal itemAllocated = txnList
                    .Where(t => t.Kind == TransactionKind.Income
                        && ((intakeItemId is not null && t.IntakeItemId == intakeItemId)
                            || (wantCategoryId is not null && t.WantCategoryId == wantCategoryId)))
                    .Sum(t => t.Amount);
                decimal itemSpent = txnList
                    .Where(t => t.Kind == TransactionKind.Expense
                        && ((intakeItemId is not null && t.IntakeItemId == intakeItemId)
                            || (wantCategoryId is not null && t.WantCategoryId == wantCategoryId)))
                    .Sum(t => t.Amount);
                decimal itemRemaining = itemAllocated - itemSpent;

                if (amount > itemRemaining)
                {
                    decimal shortfall = amount - Math.Max(0, itemRemaining);
                    willDrawFromBuffer = true;
                    bufferDrawAmount = shortfall;
                    bufferBalanceAfter = cycle.BufferBalance - shortfall; // may go negative — surfaced to caller, not clamped here
                }
            }

            decimal rawSafeToSpendAfter = safeToSpendBefore - amount;
            decimal safeToSpendAfter = Math.Max(0m, rawSafeToSpendAfter);
            decimal bucketBalanceAfter = bucketBalanceBefore - amount;
            decimal dailyLimitAfter = cycle.RemainingDays > 0 ? safeToSpendAfter / cycle.RemainingDays : 0m;

            decimal allocated = GetAllocated(bucketId, txnList);
            decimal spent = GetSpent(bucketId, txnList);
            decimal depletionPct = allocated > 0
                 ? Math.Round((spent + amount) / allocated * 100m, 1)
                 : 100m;

            var flexBuckets = bucketList.Where(b => b.Type == BucketType.Flexible && b.IsActive);
            decimal flexBudget = flexBuckets.Sum(b => GetAllocated(b.Id, txnList));
            decimal avgDailyBudget = cycle.TotalDays > 0 ? flexBudget / cycle.TotalDays : 0m;

            RiskLevel risk = GetRiskLevel(rawSafeToSpendAfter, dailyLimitAfter, avgDailyBudget, depletionPct);

            var regretSignals = new List<string>();
            if (willDrawFromBuffer)
                regretSignals.Add(bufferBalanceAfter >= 0
                    ? $"₦{bufferDrawAmount:N0} of this would come from your Buffer"
                    : "Even your Buffer can't fully cover this");
            if (dailyLimitAfter < avgDailyBudget * 0.4m)
                regretSignals.Add("Post-spend daily limit drops below 40% of cycle average");
            if (depletionPct > 90)
                regretSignals.Add($"{bucket.Name} bucket will be {depletionPct:F0}% depleted");
            if (cycle.RemainingDays <= 5 && safeToSpendAfter < dailyLimitBefore * 3)
                regretSignals.Add($"Only {cycle.RemainingDays} days left — post-spend buffer is thin");

            bool canAfford = willDrawFromBuffer ? bufferBalanceAfter >= 0 : bucketBalanceAfter >= 0;

            return new DecisionSimulationResult
            {
                BucketName = bucket.Name,
                Amount = amount,
                BucketBalanceBefore = bucketBalanceBefore,
                BucketBalanceAfter = bucketBalanceAfter,
                SafeToSpendBefore = safeToSpendBefore,
                SafeToSpendAfter = safeToSpendAfter,
                DailyLimitBefore = dailyLimitBefore,
                DailyLimitAfter = dailyLimitAfter,
                DepletionPercent = depletionPct,
                Risk = risk,
                RegretSignals = regretSignals,
                WillOverdraftBucket = bucketBalanceAfter < 0,
                WillDrawFromBuffer = willDrawFromBuffer,
                BufferDrawAmount = bufferDrawAmount,
                BufferBalanceAfter = bufferBalanceAfter,
                CurrentBalance = bucketBalanceBefore,
                PostSpendBalance = bucketBalanceAfter,
                PaceScore = pace,
                AverageDailySpend = avgDailyBudget,
                DaysRemaining = cycle.RemainingDays,
                CanAfford = canAfford
            };
        }

        // ─── WEEKLY PROJECTION ───

        public IEnumerable<WeeklyProjectionItem> ProjectWeek(IEnumerable<Bucket> buckets,
            IEnumerable<Transaction> txns, BudgetCycle cycle)
        {
            var txnList = txns.ToList();
            var bucketList = buckets.ToList();

            decimal safeNow = GetSafeToSpend(bucketList, txnList);
            decimal pace = GetSpendingPace(bucketList, txnList, cycle);
            decimal daily = GetDailyLimit(safeNow, pace, cycle);

            var projections = new List<WeeklyProjectionItem>();
            decimal runningBalance = safeNow;
            var today = DateTime.UtcNow.Date;

            for (int i = 0; i < 7; i++)
            {
                var date = today.AddDays(i);
                if (i > 0) runningBalance = Math.Max(0m, runningBalance - daily);

                projections.Add(new WeeklyProjectionItem
                {
                    Label = date.ToString("ddd"),
                    Date = date,
                    ProjectedBalance = Math.Round(runningBalance, 2),
                    ProjectedSafeToSpend = Math.Round(runningBalance, 2)
                });
            }

            return projections;
        }
    }
    
}

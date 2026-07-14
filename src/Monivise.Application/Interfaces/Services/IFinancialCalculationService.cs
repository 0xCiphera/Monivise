using Monivise.Application.DTOs.Dashboard;
using Monivise.Domain.Entities;
using Monivise.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Application.Interfaces.Services
{
    public interface IFinancialCalculationService
    {
        decimal GetAllocated(Guid bucketId, IEnumerable<Transaction> txns);
        decimal GetSpent(Guid bucketId, IEnumerable<Transaction> txns);
        decimal GetBalance(Guid bucketId, IEnumerable<Transaction> txns);
        decimal GetSafeToSpend(IEnumerable<Bucket> buckets, IEnumerable<Transaction> txns);
        decimal GetSpendingPace(IEnumerable<Bucket> buckets, IEnumerable<Transaction> txns, BudgetCycle cycle);
        decimal GetDailyLimit(decimal safeToSpend, decimal pace, BudgetCycle cycle);
        IEnumerable<AllocationPreview> AllocateIncome(decimal amount, IEnumerable<Bucket> activeBuckets);
        DecisionSimulationResult SimulateDecision(Guid bucketId, Guid? intakeItemId, Guid? wantCategoryId,
    decimal amount, IEnumerable<Bucket> buckets, IEnumerable<Transaction> txns, BudgetCycle cycle);
        RiskLevel GetRiskLevel(decimal postSafeToSpend, decimal postDailyLimit,
            decimal avgDailyBudget, decimal depletionPercent);
        IEnumerable<WeeklyProjectionItem> ProjectWeek(IEnumerable<Bucket> buckets,
            IEnumerable<Transaction> txns, BudgetCycle cycle);
    }
}

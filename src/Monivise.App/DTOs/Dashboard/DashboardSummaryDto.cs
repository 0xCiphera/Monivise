using Monivise.App.DTOs.Buckets;
using Monivise.App.DTOs.Transactions;

namespace Monivise.App.DTOs.Dashboard
{
    public record DashboardSummaryDto
        (decimal TotalIncome, decimal TotalSpent, decimal SafeToSpend, decimal DailyLimit,
    decimal PaceScore, string PaceLabel, int DaysRemaining, int DaysElapsed, int DaysInCycle, bool ActiveCycle,
    List<BucketDto> Buckets, List<TransactionDto> RecentTransactions);


}



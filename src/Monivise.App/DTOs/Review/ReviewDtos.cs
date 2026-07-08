using Monivise.App.DTOs.Buckets;

namespace Monivise.App.DTOs.Review
{
public record WeeklyReviewDto(decimal ProjectedEndBalance, decimal DailyAverage, string PaceLabel,
    List<BucketDto> BucketBreakdown, decimal Underspend);
public record SweepRequest(decimal Amount, Guid TargetGoalId);
}

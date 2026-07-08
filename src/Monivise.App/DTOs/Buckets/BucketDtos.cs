namespace Monivise.App.DTOs.Buckets
{
    public record BucketDto(Guid Id, string Name, string Icon, string Color, string Type, decimal AllocationPercent,
    bool IsActive, int DisplayOrder, decimal Allocated, decimal Spent, decimal Balance);
}

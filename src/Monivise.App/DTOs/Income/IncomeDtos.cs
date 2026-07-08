namespace Monivise.App.DTOs.Income
{
    public record AllocateIncomeRequest(decimal Amount, string Source, bool IsPrimary);
    public record AllocationResult(Guid BucketId, string BucketName, decimal Amount);
}
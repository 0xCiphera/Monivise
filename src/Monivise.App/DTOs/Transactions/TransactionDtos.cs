namespace Monivise.App.DTOs.Transactions
{
public record TransactionDto(Guid Id, string Kind, Guid? BucketId, string? BucketName, string? BucketIcon,
    string? BucketColor, decimal Amount, DateTimeOffset Date, string? Note);
public record RecordIncomeRequest(decimal Amount, string Source, bool IsPrimary);
public record RecordExpenseRequest(Guid BucketId, decimal Amount, string? Note);
}

using FluentAssertions;
using Monivise.Application.Services;
using Monivise.Domain.Entities;
using Monivise.Domain.Enums;

namespace Monivise.UnitTests.Financial;

public class IncomeAllocationTests
{
    private readonly FinancialCalculationService _svc = new();

    [Fact]
    public void AllocateIncome_SplitsProportionally()
    {
        var b1 = Bucket.Create(Guid.NewGuid(), "Food", "🍱", "#000", BucketType.Flexible, 25, 0);
        var b2 = Bucket.Create(Guid.NewGuid(), "Rent", "🏠", "#000", BucketType.Fixed, 75, 1);

        var result = _svc.AllocateIncome(100_000m, [b1, b2]).ToList();
        result.Sum(r => r.Amount).Should().Be(100_000m);
    }

    [Fact]
    public void AllocateIncome_RoundingRemainsWithLastBucket()
    {
        var b1 = Bucket.Create(Guid.NewGuid(), "A", "💰", "#000", BucketType.Flexible, 33, 0);
        var b2 = Bucket.Create(Guid.NewGuid(), "B", "💰", "#000", BucketType.Flexible, 33, 1);
        var b3 = Bucket.Create(Guid.NewGuid(), "C", "💰", "#000", BucketType.Flexible, 34, 2);

        var result = _svc.AllocateIncome(100m, [b1, b2, b3]).ToList();
        result.Sum(r => r.Amount).Should().Be(100m);
    }

    [Fact]
    public void AllocateIncome_EmptyBuckets_ReturnsEmpty()
    {
        _svc.AllocateIncome(100_000m, []).Should().BeEmpty();
    }
}
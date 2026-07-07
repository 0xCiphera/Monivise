using FluentAssertions;
using Monivise.Application.Services;
using Monivise.Domain.Entities;
using Monivise.Domain.Enums;

namespace Monivise.UnitTests.Financial;

public class SafeToSpendTests
{
    private readonly FinancialCalculationService _svc = new();

    private static Bucket MakeBucket(BucketType type, decimal pct, bool active = true)
    {
        var b = Bucket.Create(Guid.NewGuid(), "Test", "💰", "#00CFA8", type, pct, 0);
        if (!active) b.Deactivate();
        return b;
    }

    private static Transaction Inc(Guid bucketId, Guid cycleId, decimal amount)
        => Transaction.CreateIncome(Guid.NewGuid(), bucketId, cycleId, amount, "Salary");

    private static Transaction Exp(Guid bucketId, Guid cycleId, decimal amount)
        => Transaction.CreateExpense(Guid.NewGuid(), bucketId, cycleId, amount, "Test");

    [Fact]
    public void SafeToSpend_WithNoTransactions_ReturnsZero()
    {
        var buckets = new[] { MakeBucket(BucketType.Flexible, 100) };
        _svc.GetSafeToSpend(buckets, []).Should().Be(0);
    }

    [Fact]
    public void SafeToSpend_PureFlexible_EqualsRemainingBalance()
    {
        var b = MakeBucket(BucketType.Flexible, 100);
        var cycleId = Guid.NewGuid();
        var txns = new[] { Inc(b.Id, cycleId, 100_000m), Exp(b.Id, cycleId, 30_000m) };
        _svc.GetSafeToSpend([b], txns).Should().Be(70_000m);
    }

    [Fact]
    public void SafeToSpend_FixedUnpaidDeductedFromFlexible()
    {
        var flex = MakeBucket(BucketType.Flexible, 75);
        var fixed_ = MakeBucket(BucketType.Fixed, 25);
        var cycleId = Guid.NewGuid();

        var txns = new[]
        {
            Inc(flex.Id, cycleId, 75_000m),
            Exp(flex.Id, cycleId, 20_000m),
            Inc(fixed_.Id, cycleId, 25_000m)
        };

        _svc.GetSafeToSpend([flex, fixed_], txns).Should().Be(30_000m);
    }

    [Fact]
    public void SafeToSpend_SavingsBucketsAreCompletelyIgnored()
    {
        var flex = MakeBucket(BucketType.Flexible, 75);
        var savings = MakeBucket(BucketType.Savings, 25);
        var cycleId = Guid.NewGuid();

        var txns = new[]
        {
            Inc(flex.Id, cycleId, 75_000m),
            Inc(savings.Id, cycleId, 25_000m)
        };

        _svc.GetSafeToSpend([flex, savings], txns).Should().Be(75_000m);
    }

    [Fact]
    public void SafeToSpend_NeverGoesNegative()
    {
        var flex = MakeBucket(BucketType.Flexible, 50);
        var fixed_ = MakeBucket(BucketType.Fixed, 50);
        var cycleId = Guid.NewGuid();

        var txns = new[] { Inc(fixed_.Id, cycleId, 50_000m) };
        _svc.GetSafeToSpend([flex, fixed_], txns).Should().Be(0m);
    }

    [Fact]
    public void SafeToSpend_OverspentFixedDoesNotIncreaseAvailability()
    {
        var flex = MakeBucket(BucketType.Flexible, 70);
        var fixed_ = MakeBucket(BucketType.Fixed, 30);
        var cycleId = Guid.NewGuid();

        var txns = new[]
        {
            Inc(flex.Id, cycleId, 70_000m),
            Inc(fixed_.Id, cycleId, 30_000m),
            Exp(fixed_.Id, cycleId, 35_000m)
        };

        _svc.GetSafeToSpend([flex, fixed_], txns).Should().Be(70_000m);
    }

    [Fact]
    public void SafeToSpend_InactiveBucketsAreIgnored()
    {
        var active = MakeBucket(BucketType.Flexible, 100);
        var inactive = MakeBucket(BucketType.Flexible, 100, active: false);
        var cycleId = Guid.NewGuid();

        var txns = new[]
        {
            Inc(active.Id, cycleId, 50_000m),
            Inc(inactive.Id, cycleId, 50_000m)
        };

        _svc.GetSafeToSpend([active, inactive], txns).Should().Be(50_000m);
    }
}
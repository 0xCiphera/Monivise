using FluentAssertions;
using Monivise.Application.Services;
using Monivise.Domain.Entities;
using Monivise.Domain.Enums;

namespace Monivise.UnitTests.Financial;

public class DecisionSimulationTests
{
    private readonly FinancialCalculationService _svc = new();

    [Fact]
    public void SimulateDecision_ReflectsCorrectRiskLevel_Critical()
    {
        var flex = Bucket.Create(Guid.NewGuid(), "Food", "🍱", "#000", BucketType.Flexible, 100, 0);
        var cycleId = Guid.NewGuid();
        var cycle = BudgetCycle.Create(Guid.NewGuid(), DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(29));
        var txns = new[] { Transaction.CreateIncome(flex.UserId, flex.Id, cycleId, 50_000m, "Salary") };

        var result = _svc.SimulateDecision(flex.Id, 60_000m, [flex], txns, cycle);

        result.Risk.Should().Be(RiskLevel.Critical);
        result.WillOverdraftBucket.Should().BeTrue();
        result.CanAfford.Should().BeFalse();
    }

    [Fact]
    public void SimulateDecision_SmallSpend_IsRiskLevelSafe()
    {
        var flex = Bucket.Create(Guid.NewGuid(), "Food", "🍱", "#000", BucketType.Flexible, 100, 0);
        var cycleId = Guid.NewGuid();
        var cycle = BudgetCycle.Create(Guid.NewGuid(), DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(29));
        var txns = new[] { Transaction.CreateIncome(flex.UserId, flex.Id, cycleId, 200_000m, "Salary") };

        var result = _svc.SimulateDecision(flex.Id, 1_000m, [flex], txns, cycle);

        result.Risk.Should().Be(RiskLevel.Safe);
        result.WillOverdraftBucket.Should().BeFalse();
        result.CanAfford.Should().BeTrue();
    }

    [Fact]
    public void SimulateDecision_PopulatesFrontendSyncFields()
    {
        var flex = Bucket.Create(Guid.NewGuid(), "Food", "🍱", "#000", BucketType.Flexible, 100, 0);
        var cycleId = Guid.NewGuid();
        var cycle = BudgetCycle.Create(Guid.NewGuid(), DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(29));
        var txns = new[] { Transaction.CreateIncome(flex.UserId, flex.Id, cycleId, 100_000m, "Salary") };

        var result = _svc.SimulateDecision(flex.Id, 10_000m, [flex], txns, cycle);

        result.CurrentBalance.Should().Be(100_000m);
        result.PostSpendBalance.Should().Be(90_000m);
        result.PaceScore.Should().Be(0m);
        result.DaysRemaining.Should().Be(cycle.RemainingDays);
    }
}
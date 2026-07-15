using FluentAssertions;
using Monivise.Application.Services;
using Monivise.Domain.Entities;

namespace Monivise.UnitTests.Financial;

public class DailyLimitTests
{
    private readonly FinancialCalculationService _svc = new();

    private BudgetCycle MakeCycle(int daysElapsed, int totalDays)
    {
        var start = DateTime.UtcNow.Date.AddDays(-daysElapsed + 1);
        var end = start.AddDays(totalDays - 1);
        return BudgetCycle.Create(Guid.NewGuid(), start, end);
    }

    [Theory]
    [InlineData(0.70, 1.15)]
    [InlineData(0.90, 1.00)]
    [InlineData(1.10, 0.85)]
    [InlineData(1.30, 0.70)]
    public void DailyLimit_PaceAdjustmentFactorApplied(double pace, double expectedFactor)
    {
        var cycle = MakeCycle(15, 30);
        decimal safe = 15_000m;
        decimal days = cycle.RemainingDays;
        decimal limit = _svc.GetDailyLimit(safe, (decimal)pace, cycle);
        decimal expected = Math.Max(0, safe / days * (decimal)expectedFactor);
        limit.Should().BeApproximately(expected, 0.01m);
    }

    [Fact]
    public void DailyLimit_WhenNoDaysRemaining_ReturnsZero()
    {
        var start = DateTime.UtcNow.Date.AddDays(-31);
        var cycle = BudgetCycle.Create(Guid.NewGuid(), start, start.AddDays(29));
        _svc.GetDailyLimit(10_000m, 1.0m, cycle).Should().Be(0m);
    }
}
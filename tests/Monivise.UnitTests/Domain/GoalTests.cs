using FluentAssertions;
using Monivise.Domain.Entities;
using Monivise.Domain.Enums;

namespace Monivise.UnitTests.Domain;

public class GoalTests
{
    [Fact]
    public void Create_NegativeOrZeroTarget_Throws()
    {
        // Arrange/Act — wrap the call so we can assert it throws
        var act = () => Goal.Create(Guid.NewGuid(), "Trip", 0);
        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Contribute_BelowTarget_StaysActive()
    {
        var g = Goal.Create(Guid.NewGuid(), "Trip", 100_000m);
        g.Contribute(40_000m);
        g.Status.Should().Be(GoalStatus.Active);
        g.ProgressPercent.Should().Be(40m);
    }

    [Fact]
    public void Contribute_ReachesTarget_CompletesAndClampsAmount()
    {
        var g = Goal.Create(Guid.NewGuid(), "Trip", 100_000m);
        g.Contribute(150_000m); // overshoot
        g.Status.Should().Be(GoalStatus.Completed);
        g.CurrentAmount.Should().Be(100_000m); // clamped, not 150k
        g.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Contribute_ToCompletedGoal_Throws()
    {
        var g = Goal.Create(Guid.NewGuid(), "Trip", 10_000m);
        g.Contribute(10_000m); // completes it
        var act = () => g.Contribute(1_000m);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Contribute_ZeroOrNegative_Throws()
    {
        var g = Goal.Create(Guid.NewGuid(), "Trip", 10_000m);
        var act = () => g.Contribute(0);
        act.Should().Throw<ArgumentException>();
    }
}
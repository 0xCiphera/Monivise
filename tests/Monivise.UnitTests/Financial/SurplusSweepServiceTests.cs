using FluentAssertions;
using Moq;
using Monivise.Application.DTOs.Review;
using Monivise.Application.Interfaces.Repositories;
using Monivise.Application.Interfaces.Services;
using Monivise.Application.Services;
using Monivise.Domain.Entities;
using Monivise.Domain.Enums;
using Xunit;

namespace Monivise.UnitTests.Financial;

public class SurplusSweepServiceTests
{
    private readonly Mock<IBudgetCycleRepository> _cycles = new();
    private readonly Mock<IBucketRepository> _buckets = new();
    private readonly Mock<IIntakeProfileRepository> _intakes = new();
    private readonly Mock<IGoalRepository> _goals = new();
    private readonly Mock<IFinancialCalculationService> _finance = new();
    private readonly Guid _userId = Guid.NewGuid();

    private SurplusSweepService MakeService() => new(
        _cycles.Object, _buckets.Object, _intakes.Object, _goals.Object, _finance.Object);

    [Fact]
    public async Task BuildReviewAsync_NoProfile_ReturnsEmptyFixedPrompts()
    {
        _intakes.Setup(r => r.GetByUserIdAsync(_userId, default)).ReturnsAsync((IntakeProfile?)null);
        _cycles.Setup(r => r.GetActiveByUserIdAsync(_userId, default)).ReturnsAsync((BudgetCycle?)null);
        _goals.Setup(r => r.GetActiveAsync(_userId, default)).ReturnsAsync((Goal?)null);

        var result = await MakeService().BuildReviewAsync(_userId);

        result.FixedPrompts.Should().BeEmpty();
        result.ActiveGoal.Should().BeNull();
    }

    [Fact]
    public async Task BuildReviewAsync_HardFixedOnly_ReservedIsMonthlyDivByFour()
    {
        var profile = IntakeProfile.Create(_userId, 200_000m);
        profile.AddItem(IntakeItem.Create(profile.Id, "Rent", ExpenseCategory.Need, ItemNature.HardFixed, 100_000m));
        profile.AddItem(IntakeItem.Create(profile.Id, "Fun", ExpenseCategory.Want, ItemNature.Soft, 20_000m)); // excluded

        _intakes.Setup(r => r.GetByUserIdAsync(_userId, default)).ReturnsAsync(profile);
        _cycles.Setup(r => r.GetActiveByUserIdAsync(_userId, default)).ReturnsAsync((BudgetCycle?)null);
        _goals.Setup(r => r.GetActiveAsync(_userId, default)).ReturnsAsync((Goal?)null);

        var result = await MakeService().BuildReviewAsync(_userId);

        result.FixedPrompts.Should().HaveCount(1);
        result.FixedPrompts[0].Reserved.Should().Be(25_000m);
    }

    [Fact]
    public async Task BuildReviewAsync_ActiveGoal_MapsGoalRefWithProgress()
    {
        var goal = Goal.Create(_userId, "Laptop", 500_000m);
        goal.Contribute(100_000m);

        _intakes.Setup(r => r.GetByUserIdAsync(_userId, default)).ReturnsAsync((IntakeProfile?)null);
        _cycles.Setup(r => r.GetActiveByUserIdAsync(_userId, default)).ReturnsAsync((BudgetCycle?)null);
        _goals.Setup(r => r.GetActiveAsync(_userId, default)).ReturnsAsync(goal);

        var result = await MakeService().BuildReviewAsync(_userId);

        result.ActiveGoal!.ProgressPercent.Should().Be(20m);
    }

    [Fact]
    public async Task ApplySweepAsync_GoalNotFound_Throws()
    {
        _goals.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Goal?)null);
        var dto = new ApplySweepDto { GoalId = Guid.NewGuid(), Amount = 1000m };

        var act = async () => await MakeService().ApplySweepAsync(_userId, dto);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ApplySweepAsync_GoalOwnedByAnotherUser_Throws()
    {
        var goal = Goal.Create(Guid.NewGuid(), "Trip", 50_000m); // not _userId
        _goals.Setup(r => r.GetByIdAsync(goal.Id, default)).ReturnsAsync(goal);
        var dto = new ApplySweepDto { GoalId = goal.Id, Amount = 1000m };

        var act = async () => await MakeService().ApplySweepAsync(_userId, dto);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task ApplySweepAsync_Valid_ContributesAndSaves()
    {
        var goal = Goal.Create(_userId, "Trip", 50_000m);
        _goals.Setup(r => r.GetByIdAsync(goal.Id, default)).ReturnsAsync(goal);
        var dto = new ApplySweepDto { GoalId = goal.Id, Amount = 10_000m };

        await MakeService().ApplySweepAsync(_userId, dto);

        goal.CurrentAmount.Should().Be(10_000m);
        _goals.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }
}
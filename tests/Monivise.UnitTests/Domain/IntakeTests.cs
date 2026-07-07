using FluentAssertions;
using Monivise.Domain.Entities;
using Monivise.Domain.Enums;
using Xunit;

namespace Monivise.UnitTests.Domain;

public class IntakeItemTests
{
    [Fact]
    public void Create_UnpricedNature_ForcesAmountToZero()
    {
        var item = IntakeItem.Create(Guid.NewGuid(), "Someday trip",
            ExpenseCategory.Want, ItemNature.Unpriced, 500_000m);
        item.MonthlyAmount.Should().Be(0m);
    }

    [Fact]
    public void Create_NegativeAmount_Throws()
    {
        var act = () => IntakeItem.Create(Guid.NewGuid(), "Rent",
            ExpenseCategory.Need, ItemNature.HardFixed, -1m);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Create_TrimsName()
    {
        var item = IntakeItem.Create(Guid.NewGuid(), "  Rent  ",
            ExpenseCategory.Need, ItemNature.HardFixed, 1000m);
        item.Name.Should().Be("Rent");
    }
}

public class IntakeProfileTests
{
    [Fact]
    public void Create_NegativeBaseline_Throws()
    {
        var act = () => IntakeProfile.Create(Guid.NewGuid(), -1m);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ChoosePathway_SetsCompleteAndPathway()
    {
        var p = IntakeProfile.Create(Guid.NewGuid(), 100_000m);
        p.ChoosePathway(PathwayType.Moderate);
        p.IsComplete.Should().BeTrue();
        p.ChosenPathway.Should().Be(PathwayType.Moderate);
    }

    [Fact]
    public void ClearItems_RemovesAllPreviouslyAddedItems()
    {
        var p = IntakeProfile.Create(Guid.NewGuid(), 100_000m);
        p.AddItem(IntakeItem.Create(p.Id, "Rent", ExpenseCategory.Need, ItemNature.HardFixed, 1000m));
        p.ClearItems();
        p.Items.Should().BeEmpty();
    }
}
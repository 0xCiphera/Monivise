using FluentAssertions;
using Monivise.Application.Services;
using Monivise.Domain.Entities;
using Monivise.Domain.Enums;

namespace Monivise.UnitTests.Financial;

public class AllocationRecommendationServiceTests
{
    private readonly AllocationRecommendationService _svc = new();

    private static IntakeProfile MakeProfile(decimal income, params IntakeItem[] items)
    {
        var p = IntakeProfile.Create(Guid.NewGuid(), income);
        foreach (var i in items) p.AddItem(i);
        return p;
    }

    private static IntakeItem Item(ExpenseCategory cat, ItemNature nature, decimal amt, bool reserveOnly = false)
        => IntakeItem.Create(Guid.NewGuid(), "x", cat, nature, amt, reserveOnly);

    [Fact]
    public void BuildPathways_ReturnsExactlyThreePathways()
    {
        var profile = MakeProfile(200_000m);
        _svc.BuildPathways(profile).Should().HaveCount(3);
    }

    [Fact]
    public void BuildPathways_CommittedExceedsIncome_MarksUnaffordableWithGap()
    {
        var profile = MakeProfile(100_000m,
            Item(ExpenseCategory.Need, ItemNature.HardFixed, 90_000m),
            Item(ExpenseCategory.Want, ItemNature.Soft, 50_000m)); // 140k committed > 100k income

        var prudent = _svc.BuildPathways(profile).First(p => p.Pathway == "Prudent");

        prudent.IsAffordable.Should().BeFalse();
        prudent.AffordabilityGap.Should().BeGreaterThan(0);
        prudent.SuggestedCuts.Should().NotBeEmpty();
    }

    [Fact]
    public void BuildPathways_AffordableCase_BucketsSumToExactly100()
    {
        var profile = MakeProfile(300_000m,
            Item(ExpenseCategory.Need, ItemNature.HardFixed, 100_000m),
            Item(ExpenseCategory.Want, ItemNature.Soft, 50_000m),
            Item(ExpenseCategory.Investment, ItemNature.Soft, 20_000m));

        foreach (var pathway in _svc.BuildPathways(profile))
        {
            pathway.Buckets.Sum(b => b.AllocationPercent).Should().Be(100m,
                $"{pathway.Pathway} bucket percentages must sum to exactly 100");
        }
    }

    [Fact]
    public void BuildPathways_UnpricedItem_ContributesZeroCost()
    {
        // Nature=Unpriced forces MonthlyAmount to 0 in IntakeItem.Create — verify the service respects that
        var profile = MakeProfile(200_000m,
            Item(ExpenseCategory.Want, ItemNature.Unpriced, 999_999m)); // amount should be discarded

        var prudent = _svc.BuildPathways(profile).First(p => p.Pathway == "Prudent");
        prudent.IsAffordable.Should().BeTrue(); // wouldn't be, if the 999,999 leaked through
    }

    [Fact]
    public void BuildPathways_ReserveOnlySoftItem_ExcludedFromSpendableSoft()
    {
        // ReserveOnly soft items are excluded from softBase — verify Prudent stays affordable
        var profile = MakeProfile(100_000m,
            Item(ExpenseCategory.Want, ItemNature.Soft, 80_000m, reserveOnly: true));

        var prudent = _svc.BuildPathways(profile).First(p => p.Pathway == "Prudent");
        prudent.IsAffordable.Should().BeTrue(); // 80k excluded, so income easily covers it
    }

    [Fact]
    public void BuildPathways_ComfortableSavesLessThanPrudent()
    {
        var profile = MakeProfile(300_000m,
            Item(ExpenseCategory.Need, ItemNature.HardFixed, 100_000m),
            Item(ExpenseCategory.Want, ItemNature.Soft, 50_000m));

        var results = _svc.BuildPathways(profile).ToDictionary(p => p.Pathway);

        results["Comfortable"].MonthlySavings.Should()
            .BeLessThan(results["Prudent"].MonthlySavings);
    }

    [Fact]
    public void BuildPathways_DailyAndWeeklyLimit_AreConsistent()
    {
        var profile = MakeProfile(200_000m,
            Item(ExpenseCategory.Need, ItemNature.HardFixed, 60_000m));

        var moderate = _svc.BuildPathways(profile).First(p => p.Pathway == "Moderate");
        moderate.WeeklyLimit.Should().BeApproximately(moderate.DailyLimit * 7m, 0.01m);
    }
}
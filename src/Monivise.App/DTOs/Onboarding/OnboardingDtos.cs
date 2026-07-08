namespace Monivise.App.DTOs.Onboarding
{
    public record ExpenseIntake(string Name, string Nature, decimal? Amount); // Nature: HardFixed | Soft | Unpriced
    public record OnboardingIntakeRequest(decimal MonthlyIncome, List<ExpenseIntake> Expenses);
    public record PathwayPreview(string Pathway, decimal DailyLimit, decimal WeeklyLimit, decimal SavingsRate,
        string Example, bool IsAffordable, decimal? Gap, List<string>? SuggestedCuts);
    public record OnboardingCommitRequest(decimal MonthlyIncome, List<ExpenseIntake> Expenses, string Pathway);

}
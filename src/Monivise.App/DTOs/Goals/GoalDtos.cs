namespace Monivise.App.DTOs.Goals
{
    public record GoalDto(Guid Id, string Name, decimal TargetAmount, decimal CurrentAmount, bool IsActive);
    public record CreateGoalRequest(string Name, decimal TargetAmount);
}

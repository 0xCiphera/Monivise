namespace Monivise.App.DTOs.Simulator
{
public record SimulateRequest(Guid BucketId, decimal Amount);
public record SimulateResponse(bool CanAfford, decimal CurrentBalance, decimal PostSpendBalance, decimal PaceScore,
    decimal AverageDailySpend, int DaysRemaining, List<string> RegretSignals);
}

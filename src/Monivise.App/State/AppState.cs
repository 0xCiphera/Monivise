using Fluxor;
using Monivise.App.DTOs;
using Monivise.App.DTOs.Buckets;
using Monivise.App.DTOs.Dashboard;
using Monivise.App.DTOs.Simulator;
using Monivise.App.DTOs.Transactions;

namespace Monivise.App.State;

public record AppMessage(Guid Id, string Text, string Variant);

[FeatureState]
public record AppState
{
    public bool IsLoading { get; init; }
    public DashboardSummaryDto? Dashboard { get; init; }
    public List<BucketDto> Buckets { get; init; } = new();
    public List<TransactionDto> Transactions { get; init; } = new();
    public List<AppMessage> Messages { get; init; } = new();
    public Guid? SimulatorBucketId { get; init; }
    public decimal SimulatorAmount { get; init; }
    public SimulateResponse? SimulatorPreview { get; init; }
    public bool SimulatorIsLoading { get; init; }
    public string? SimulatorError { get; init; }

    private AppState() { }
    public static AppState CreateInitial() => new();
}

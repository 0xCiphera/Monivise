using Monivise.App.DTOs;

namespace Monivise.App.API;

public class BudgetCycleApiClient : ApiClient
{
    public BudgetCycleApiClient(HttpClient http) : base(http) { }
    public Task<CycleDto?> GetActiveAsync() => GetAsync<CycleDto?>("api/cycles/active");
    public Task<RolloverResponse> RolloverAsync(RolloverRequest req) =>
        PostAsync<RolloverResponse>("api/cycles/rollover", req);
}
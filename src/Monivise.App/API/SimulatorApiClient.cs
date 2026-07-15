using Monivise.App.DTOs;

namespace Monivise.App.API;

public class SimulatorApiClient : ApiClient
{
    public SimulatorApiClient(HttpClient http) : base(http) { }
    public Task<SimulateResponse> PreviewAsync(SimulateRequest req) =>
        PostAsync<SimulateResponse>("api/simulator/preview", req);
    public Task<TransactionDto> CommitAsync(SimulateRequest req) =>
        PostAsync<TransactionDto>("api/simulator/commit", req);
}

using Monivise.App.DTOs;

namespace Monivise.App.API;

public class CycleApiClient : ApiClient
{
    public CycleApiClient(HttpClient http) : base(http) { }

    public Task<CycleDto> StartCycleAsync() => PostAsync<CycleDto>("api/cycles/start", null);
    public Task<CycleDto?> GetActiveAsync() => GetAsync<CycleDto?>("api/cycles/active");
}

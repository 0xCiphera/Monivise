using Monivise.App.DTOs.Income;

namespace Monivise.App.API;

public class IncomeApiClient : ApiClient
{
    public IncomeApiClient(HttpClient http) : base(http) { }
    public Task<List<AllocationResult>> AllocateAsync(AllocateIncomeRequest req) =>
        PostAsync<List<AllocationResult>>("api/income/allocate", req);
}

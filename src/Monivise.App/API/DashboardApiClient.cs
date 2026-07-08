using Monivise.App.DTOs;
using Monivise.App.DTOs.Dashboard;

namespace Monivise.App.API;

public class DashboardApiClient : ApiClient
{
    public DashboardApiClient(HttpClient http) : base(http) { }
    public Task<DashboardSummaryDto> GetSummaryAsync() => GetAsync<DashboardSummaryDto>("api/dashboard/summary");
}

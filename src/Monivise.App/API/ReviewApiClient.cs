using Monivise.App.DTOs;

namespace Monivise.App.API;

public class ReviewApiClient : ApiClient
{
    public ReviewApiClient(HttpClient http) : base(http) { }
    public Task<WeeklyReview> GetWeeklyAsync() => GetAsync<WeeklyReview>("api/review/weekly");
    public Task SweepAsync(ApplySweepRequest req) => PostAsync("api/review/sweep", req);
}

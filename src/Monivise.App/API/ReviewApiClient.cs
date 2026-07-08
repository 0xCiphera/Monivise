using Monivise.App.DTOs;
using Monivise.App.DTOs.Review;

namespace Monivise.App.API;

public class ReviewApiClient : ApiClient
{
    public ReviewApiClient(HttpClient http) : base(http) { }
    public Task<WeeklyReviewDto> GetWeeklyAsync() => GetAsync<WeeklyReviewDto>("api/review/weekly");
    public Task SweepAsync(SweepRequest req) => PostAsync("api/review/sweep", req);
}

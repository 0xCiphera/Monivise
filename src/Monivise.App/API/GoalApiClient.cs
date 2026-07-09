using Monivise.App.DTOs;

namespace Monivise.App.API;

public class GoalApiClient : ApiClient
{
    public GoalApiClient(HttpClient http) : base(http) { }
    public Task<List<GoalDto>> GetGoalsAsync() => GetAsync<List<GoalDto>>("api/goals");
    public Task<GoalDto> CreateAsync(CreateGoalRequest req) => PostAsync<GoalDto>("api/goals", req);
    public Task<GoalDto> ActivateAsync(Guid id) => PostAsync<GoalDto>($"api/goals/{id}/activate");
}

using Monivise.App.DTOs;

namespace Monivise.App.API;

public class OnboardingApiClient : ApiClient
{
    public OnboardingApiClient(HttpClient http) : base(http) { }
    public Task<List<PathwayPreview>> IntakeAsync(OnboardingIntakeRequest req) =>
        PostAsync<List<PathwayPreview>>("api/onboarding/intake", req);
    public Task CommitAsync(CommitPathwayRequest req) => PostAsync("api/onboarding/commit", req);
}

using Monivise.App.DTOs;
using Monivise.App.DTOs.Onboarding;

namespace Monivise.App.API;

public class OnboardingApiClient : ApiClient
{
    public OnboardingApiClient(HttpClient http) : base(http) { }
    public Task<List<PathwayPreview>> IntakeAsync(OnboardingIntakeRequest req) =>
        PostAsync<List<PathwayPreview>>("api/onboarding/intake", req);
    public Task CommitAsync(OnboardingCommitRequest req) => PostAsync("api/onboarding/commit", req);
}

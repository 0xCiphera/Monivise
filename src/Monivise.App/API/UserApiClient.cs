using Monivise.App.DTOs;

namespace Monivise.App.API;

public class UserApiClient : ApiClient
{
    public UserApiClient(HttpClient http) : base(http) { }

    public Task<UserProfileDto> GetProfileAsync() => GetAsync<UserProfileDto>("api/user/profile");
    public Task<UserProfileDto> UpdateProfileAsync(UpdateProfileRequest req) => PutAsync<UserProfileDto>("api/user/profile", req);
}

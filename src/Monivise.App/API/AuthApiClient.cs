using Monivise.App.DTOs;

namespace Monivise.App.API;

public class AuthApiClient : ApiClient
{
    public AuthApiClient(HttpClient http) : base(http) { }
    public Task<AuthResponse> LoginAsync(LoginRequest req) => PostAsync<AuthResponse>("api/auth/login", req);
    public Task<AuthResponse> RegisterAsync(RegisterRequest req) => PostAsync<AuthResponse>("api/auth/register", req);
    public Task<AuthResponse> RefreshAsync() => PostAsync<AuthResponse>("api/auth/refresh");
    public Task LogoutAsync() => PostAsync("api/auth/logout");
}

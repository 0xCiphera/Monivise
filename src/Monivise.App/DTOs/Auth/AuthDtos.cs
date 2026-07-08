namespace Monivise.App.DTOs.Auth 
{

    public record LoginRequest(string Email, string Password);
    public record RegisterRequest(string DisplayName, string Email, string Password, string Currency);
    public record AuthResponse(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt, Guid UserId, string DisplayName, string Currency);
}


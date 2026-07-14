using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace Monivise.App.Authentication;

public class JwtAuthStateProvider : AuthenticationStateProvider
{
    private readonly TokenStore _tokenStore;

    public JwtAuthStateProvider(TokenStore tokenStore)
    {
        _tokenStore = tokenStore;
        _tokenStore.Changed += () => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (string.IsNullOrWhiteSpace(_tokenStore.AccessToken) || _tokenStore.IsExpired)
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));

        var claims = ParseClaims(_tokenStore.AccessToken);
        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"))));
    }

    private static IEnumerable<Claim> ParseClaims(string jwt)
    {
        var payload = jwt.Split('.')[1].Replace('-', '+').Replace('_', '/');
        payload += (payload.Length % 4) switch { 2 => "==", 3 => "=", _ => "" };
        var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(payload));
        var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json) ?? new();
        return dict.Select(kv => new Claim(kv.Key, kv.Value.ToString()));
    }
}

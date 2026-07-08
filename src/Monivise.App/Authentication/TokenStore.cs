namespace Monivise.App.Authentication;

public class TokenStore
{
    public string? AccessToken { get; private set; }
    public DateTimeOffset? ExpiresAt { get; private set; }

    public event Action? Changed;

    public void Set(string token, DateTimeOffset expiresAt)
    {
        AccessToken = token;
        ExpiresAt = expiresAt;
        Changed?.Invoke();
    }

    public void Clear()
    {
        AccessToken = null;
        ExpiresAt = null;
        Changed?.Invoke();
    }

    public bool IsExpired => ExpiresAt is null || ExpiresAt <= DateTimeOffset.UtcNow.AddSeconds(10);
}

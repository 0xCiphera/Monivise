using Fluxor;

namespace Monivise.App.State;

[FeatureState]
public record AuthState
{
    public bool IsAuthenticated { get; init; }
    public bool IsLoading { get; init; }
    public string? DisplayName { get; init; }
    public string? ErrorMessage { get; init; }

    private AuthState() { }
    public static AuthState CreateInitial() => new();
}

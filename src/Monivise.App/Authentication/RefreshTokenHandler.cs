using Monivise.App.DTOs.Auth;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;


namespace Monivise.App.Authentication;

public class RefreshTokenHandler : DelegatingHandler
{
    private readonly TokenStore _tokenStore;
    private readonly IHttpClientFactory _factory;
    private readonly Action _onRefreshFailed;
    private bool _isRefreshing;

    public RefreshTokenHandler(TokenStore tokenStore, IHttpClientFactory factory, Action onRefreshFailed)
    {
        _tokenStore = tokenStore;
        _factory = factory;
        _onRefreshFailed = onRefreshFailed;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(_tokenStore.AccessToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tokenStore.AccessToken);

        var response = await base.SendAsync(request, ct);

        if (response.StatusCode != HttpStatusCode.Unauthorized || _isRefreshing)
            return response;

        _isRefreshing = true;
        try
        {
            var raw = _factory.CreateClient("MoniviseRawApi");
            var refreshResponse = await raw.PostAsync("api/auth/refresh", null, ct);
            if (!refreshResponse.IsSuccessStatusCode)
            {
                _tokenStore.Clear(); _onRefreshFailed(); return response;
            }

            var auth = await refreshResponse.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: ct);
            if (auth is null)
            {
                _tokenStore.Clear(); _onRefreshFailed(); return response;
            }

            _tokenStore.Set(auth.AccessToken, auth.ExpiresAt);
            var retry = await CloneAsync(request);
            retry.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
            return await base.SendAsync(retry, ct);
        }
        finally { _isRefreshing = false; }
    }

    private static async Task<HttpRequestMessage> CloneAsync(HttpRequestMessage original)
    {
        var clone = new HttpRequestMessage(original.Method, original.RequestUri);
        if (original.Content is not null)
        {
            var bytes = await original.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(bytes);
            foreach (var h in original.Content.Headers)
                clone.Content.Headers.TryAddWithoutValidation(h.Key, h.Value);
        }
        foreach (var h in original.Headers)
            clone.Headers.TryAddWithoutValidation(h.Key, h.Value);
        return clone;
    }
}

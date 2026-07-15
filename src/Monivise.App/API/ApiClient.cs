using System.Net.Http.Json;
using System.Text.Json;

namespace Monivise.App.API;

public class ApiClient
{
    protected readonly HttpClient Http;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public ApiClient(HttpClient http) => Http = http;

    protected async Task<T> GetAsync<T>(string url)
    {
        var response = await SendWithNetworkGuard(() => Http.GetAsync(url));
        await EnsureSuccessOrThrowAsync(response);
        return await DeserializeAsync<T>(response);
    }
    protected async Task PostAsync(string url, object? body = null)
    {
        var response = await SendWithNetworkGuard(() => Http.PostAsJsonAsync(url, body, JsonOptions));
        await EnsureSuccessOrThrowAsync(response);
    }

    protected async Task<T> PostAsync<T>(string url, object? body = null)
    {
        var response = await SendWithNetworkGuard(() => Http.PostAsJsonAsync(url, body, JsonOptions));
        await EnsureSuccessOrThrowAsync(response);
        return await DeserializeAsync<T>(response);
    }

    protected async Task PutAsync(string url, object? body = null)
    {
        var response = await SendWithNetworkGuard(() => Http.PutAsJsonAsync(url, body, JsonOptions));
        await EnsureSuccessOrThrowAsync(response);
    }
    protected async Task<T> PutAsync<T>(string url, object? body = null)
    {
        var response = await SendWithNetworkGuard(() => Http.PutAsJsonAsync(url, body, JsonOptions));
        await EnsureSuccessOrThrowAsync(response);
        return await DeserializeAsync<T>(response);
    }

    protected async Task DeleteAsync(string url)
    {
        var response = await SendWithNetworkGuard(() => Http.DeleteAsync(url));
        await EnsureSuccessOrThrowAsync(response);
    }

    private static async Task<HttpResponseMessage> SendWithNetworkGuard(Func<Task<HttpResponseMessage>> send)
    {
        try
        {
            return await send();
        }
        catch (HttpRequestException)
        {
            throw new ApiException(0, "Network error. Please check your connection.", "network_error");
        }
        catch (TaskCanceledException)
        {
            throw new ApiException(0, "The request timed out. Please try again.", "timeout");
        }
    }

    private static async Task<T> DeserializeAsync<T>(HttpResponseMessage response)
    {
        try
        {
            var result = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
            if (result is null)
                throw new ApiException((int)response.StatusCode, "The server returned an empty response.", "empty_response");
            return result;
        }
        catch (JsonException)
        {
            throw new ApiException((int)response.StatusCode, "The server returned an unexpected response.", "parse_error");
        }
    }

    private static async Task EnsureSuccessOrThrowAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;

        var status = (int)response.StatusCode;
        string message;
        string? errorCode = null;
        Dictionary<string, string[]>? fieldErrors = null;

        var body = await response.Content.ReadAsStringAsync();
        try
        {
            var problem = JsonSerializer.Deserialize<ApiErrorBody>(body, JsonOptions);
            message = problem?.Message ?? DefaultMessageFor(status);
            errorCode = problem?.Code;
            fieldErrors = problem?.Errors;
        }
        catch
        {
            message = DefaultMessageFor(status);
        }

        throw new ApiException(status, message, errorCode, fieldErrors);
    }

    private static string DefaultMessageFor(int status) => status switch
    {
        400 => "That request wasn't valid. Check your input and try again.",
        401 => "Your session has expired. Please sign in again.",
        403 => "You don't have permission to do that.",
        404 => "We couldn't find what you were looking for.",
        409 => "That already exists.",
        422 => "Some of the information provided isn't valid.",
        >= 500 => "Something went wrong on our end. Please try again.",
        _ => "Something went wrong. Please try again."
    };

    private record ApiErrorBody(string? Message, string? Code, Dictionary<string, string[]>? Errors);
}
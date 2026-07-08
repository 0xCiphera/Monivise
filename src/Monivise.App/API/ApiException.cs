namespace Monivise.App.API;

public class ApiException : Exception
{
    public int StatusCode { get; }
    public string? ErrorCode { get; }
    public Dictionary<string, string[]>? FieldErrors { get; }

    public ApiException(int statusCode, string message, string? errorCode = null,
        Dictionary<string, string[]>? fieldErrors = null) : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        FieldErrors = fieldErrors;
    }
}

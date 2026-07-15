using Monivise.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace Monivise.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (DomainException ex)
            {
                _logger.LogWarning("Domain exception: {Code} — {Message}", ex.Code, ex.Message);
                await WriteProblem(context, HttpStatusCode.UnprocessableEntity, ex.Code, ex.Message);
            }
            catch (ArgumentException ex)
            {
                await WriteProblem(context, HttpStatusCode.BadRequest, "INVALID_ARGUMENT", ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                await WriteProblem(context, HttpStatusCode.Unauthorized, "UNAUTHORIZED", "Unauthorized.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                await WriteProblem(context, HttpStatusCode.InternalServerError, "INTERNAL_ERROR",
                    "An unexpected error occurred. Please try again.");
            }
        }

        private static async Task WriteProblem(HttpContext ctx, HttpStatusCode status, string code, string detail)
        {
            ctx.Response.StatusCode = (int)status;
            ctx.Response.ContentType = "application/problem+json";
            var body = JsonSerializer.Serialize(new
            {
                type = $"https://monivise.app/errors/{code.ToLower()}",
                title = code,
                status = (int)status,
                detail
            });
            await ctx.Response.WriteAsync(body);
        }
    }
}

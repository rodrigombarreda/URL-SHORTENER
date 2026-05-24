using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace UrlShortener.Api.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred.");

                context.Response.ContentType = "application/json";

                var statusCode = ex switch
                {
                    ArgumentException => (int)HttpStatusCode.BadRequest,
                    KeyNotFoundException => (int)HttpStatusCode.NotFound,
                    UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                    _ => (int)HttpStatusCode.InternalServerError
                };

                context.Response.StatusCode = statusCode;

                var errorResponse = new
                {
                    code = statusCode,
                    message = ex.Message,
                    timestamp = DateTime.UtcNow
                };

                var json = JsonSerializer.Serialize(errorResponse);
                await context.Response.WriteAsync(json);
            }
        }
    }
}

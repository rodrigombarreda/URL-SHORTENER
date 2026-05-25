using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace UrlShortener.Api.Middleware
{
    // Middleware global de manejo de errores. Envuelve todo el pipeline HTTP en un try/catch
    // centralizado para que ninguna excepción no manejada llegue al cliente como un error 500
    // genérico o, peor, exponiendo detalles internos del sistema. Mapea tipos de excepción a
    // códigos HTTP específicos (ArgumentException → 400, KeyNotFoundException → 404, etc.) y
    // devuelve siempre una respuesta JSON consistente con { code, message, timestamp }.
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

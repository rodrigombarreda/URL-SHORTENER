using System.Net;
using System.Text.Json;
using Xunit;

// Pruebas de integración del ErrorHandlingMiddleware.
// Verifican que cualquier excepción no manejada en la pipeline sea capturada
// y devuelta como un JSON estructurado con el status code, mensaje y timestamp correctos.
// Requieren que la API esté corriendo en http://localhost:5000.
[Collection("IntegrationTests")]
public class MiddlewareTests : IntegrationTestBase
{
    // Verifica que el middleware intercepta una excepción no manejada y devuelve:
    // - HTTP 500
    // - Content-Type: application/json
    // - Body con los campos: code (500), message (con el texto de la excepción) y timestamp
    [Fact]
    public async Task Middleware_OnUnhandledException_ShouldReturnStructuredJsonError()
    {
        var response = await Client.GetAsync("/api/url/throw");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        Assert.Equal(500, root.GetProperty("code").GetInt32());
        Assert.Contains("Test exception", root.GetProperty("message").GetString());
        Assert.True(root.TryGetProperty("timestamp", out _));
    }
}

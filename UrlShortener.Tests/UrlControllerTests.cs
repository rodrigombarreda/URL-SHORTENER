using System.Net;
using Xunit;

// Pruebas de integración del flujo principal del UrlController.
// Verifican el happy path (crear y redirigir), el comportamiento de autenticación,
// y que el endpoint de redirect sea de acceso público.
// Requieren que la API esté corriendo en http://localhost:5000.
[Collection("IntegrationTests")]
public class UrlControllerTests : IntegrationTestBase
{
    // Verifica que al enviar una longUrl válida se obtiene un shortCode en la respuesta.
    [Fact]
    public async Task ShortenUrl_ShouldReturnShortCode()
    {
        var response = await Client.PostAsync("/api/url/shorten", JsonBody("{\"longUrl\":\"https://example.com/basic\"}"));

        response.EnsureSuccessStatusCode();
        Assert.Contains("shortUrl", await response.Content.ReadAsStringAsync());
    }

    // Verifica el flujo completo: crear una URL corta y luego resolver el redirect.
    // Se espera HTTP 302 con el header Location apuntando a la URL original.
    [Fact]
    public async Task GetLongUrl_ShouldRedirectToOriginalUrl()
    {
        var shortCode = await CreateShortCode("https://example.com/redirect-test");

        var response = await Client.GetAsync($"/api/url/{shortCode}");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal("https://example.com/redirect-test", response.Headers.Location?.ToString());
    }

    // Verifica que el endpoint de creación requiere autenticación JWT.
    // Un request sin token debe ser rechazado con 401.
    [Fact]
    public async Task ShortenUrl_WithoutToken_ShouldReturnUnauthorized()
    {
        var response = await UnauthenticatedClient.PostAsync("/api/url/shorten", JsonBody("{\"longUrl\":\"https://example.com/no-auth\"}"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // Verifica que el endpoint de redirect es público ([AllowAnonymous]).
    // Un usuario sin token debe poder seguir un link corto sin autenticarse.
    [Fact]
    public async Task GetLongUrl_WithoutToken_ShouldStillRedirect()
    {
        var shortCode = await CreateShortCode("https://example.com/public-redirect");

        var response = await UnauthenticatedClient.GetAsync($"/api/url/{shortCode}");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
    }
}

using System.Net;
using Xunit;

// Pruebas de integración que verifican el manejo de errores en todos los endpoints.
// Cubren inputs inválidos, credenciales incorrectas y recursos inexistentes,
// asegurando que la API devuelva el status code y el campo "message" correctos.
// Requieren que la API esté corriendo en http://localhost:5000.
[Collection("IntegrationTests")]
public class ErrorHandlingTests : IntegrationTestBase
{
    // Verifica que el login sin body devuelve 400.
    // El modelo requiere username y password; si faltan, el middleware de validación los rechaza.
    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenCredentialsAreMissing()
    {
        var response = await Client.PostAsync("/api/auth/login", JsonBody("{}"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("message", await response.Content.ReadAsStringAsync());
    }

    // Verifica que credenciales incorrectas devuelven 401 con mensaje de error.
    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
    {
        var response = await Client.PostAsync("/api/auth/login", JsonBody("{\"username\":\"wrong\",\"password\":\"bad\"}"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Contains("message", await response.Content.ReadAsStringAsync());
    }

    // Verifica que el endpoint de acortado rechaza requests sin el campo longUrl.
    [Fact]
    public async Task ShortenUrl_ShouldReturnBadRequest_WhenLongUrlIsMissing()
    {
        var response = await Client.PostAsync("/api/url/shorten", JsonBody("{}"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("message", await response.Content.ReadAsStringAsync());
    }

    // Verifica que una longUrl de solo espacios es rechazada.
    // El controller valida IsNullOrWhiteSpace antes de llamar al servicio.
    [Fact]
    public async Task ShortenUrl_ShouldReturnBadRequest_WhenLongUrlIsWhitespace()
    {
        var response = await Client.PostAsync("/api/url/shorten", JsonBody("{\"longUrl\":\"   \"}"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("message", await response.Content.ReadAsStringAsync());
    }

    // Verifica que un shortCode que no existe en DB ni en cache devuelve 404.
    // El ErrorHandlingMiddleware mapea KeyNotFoundException → 404.
    [Fact]
    public async Task GetLongUrl_ShouldReturnNotFound_WhenShortCodeDoesNotExist()
    {
        var response = await Client.GetAsync("/api/url/doesnotexist000");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("message", await response.Content.ReadAsStringAsync());
    }

    // Verifica que GET /api/url/ (sin shortCode) devuelve 400.
    // El endpoint GetLongUrlEmpty maneja explícitamente este caso.
    [Fact]
    public async Task GetLongUrl_ShouldReturnBadRequest_WhenShortCodeIsEmpty()
    {
        var response = await Client.GetAsync("/api/url/");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("message", await response.Content.ReadAsStringAsync());
    }
}

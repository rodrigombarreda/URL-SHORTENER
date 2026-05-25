using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

// Clase base para todos los tests de integración.
// Centraliza la configuración del HttpClient autenticado y sin autenticar,
// y expone helpers reutilizables para evitar duplicar código en cada clase de tests.
public abstract class IntegrationTestBase
{
    protected readonly HttpClient Client;
    protected readonly HttpClient UnauthenticatedClient;

    protected IntegrationTestBase()
    {
        Client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false })
        {
            BaseAddress = new Uri("http://localhost:5000")
        };
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTestHelper.GenerateTestToken());

        UnauthenticatedClient = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false })
        {
            BaseAddress = new Uri("http://localhost:5000")
        };
    }

    protected static StringContent JsonBody(string json) =>
        new(json, Encoding.UTF8, "application/json");

    protected async Task<string> CreateShortCode(string longUrl)
    {
        var response = await Client.PostAsync("/api/url/shorten", JsonBody($"{{\"longUrl\":\"{longUrl}\"}}"));
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("shortUrl").GetString()!;
    }
}

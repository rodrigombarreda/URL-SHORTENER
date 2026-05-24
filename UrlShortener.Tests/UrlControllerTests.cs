using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using System.Net;

[Collection("IntegrationTests")]
public class UrlControllerTests
{
    private readonly HttpClient _client;

    public UrlControllerTests()
    {
        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = false
        };

        _client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:5000")
        };

        var token = JwtTestHelper.GenerateTestToken();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task ShortenUrl_ShouldReturnShortCode()
    {
        var json = "{\"longUrl\":\"https://example.com/test\"}";
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/url/shorten", content);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("shortUrl", body);
    }

    [Fact]
    public async Task GetLongUrl_ShouldRedirectToOriginal()
    {
        var json = "{\"longUrl\":\"https://example.com/test-get\"}";
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/url/shorten", content);
        createResponse.EnsureSuccessStatusCode();

        var body = await createResponse.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var shortUrl = doc.RootElement.GetProperty("shortUrl").GetString();

        Assert.NotNull(shortUrl);

        var shortCode = shortUrl!.Contains("/")
            ? shortUrl.Split('/').Last()
            : shortUrl;

        shortCode = shortCode.Trim();
        var getResponse = await _client.GetAsync($"/api/url/{shortCode}");

        Assert.Equal(System.Net.HttpStatusCode.Found, getResponse.StatusCode);
        Assert.Equal("https://example.com/test-get", getResponse.Headers.Location?.ToString());
    }

    [Fact]
    public async Task ShortenUrl_1000ParallelRequests()
    {
        var semaphore = new SemaphoreSlim(50, 50);
        var tasks = Enumerable.Range(0, 1000).Select(async i =>
        {
            await semaphore.WaitAsync();
            try
            {
                var json = $"{{\"longUrl\":\"https://example.com/test-{i}\"}}";
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _client.PostAsync("/api/url/shorten", content);
                response.EnsureSuccessStatusCode();
            }
            finally { semaphore.Release(); }
        });

        await Task.WhenAll(tasks);
    }

    [Fact]
    public async Task GetLongUrl_1000ParallelRequests()
    {
        // Crear una URL primero
        var json = "{\"longUrl\":\"https://example.com/load-get\"}";
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/url/shorten", content);
        createResponse.EnsureSuccessStatusCode();

        var body = await createResponse.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var shortUrl = doc.RootElement.GetProperty("shortUrl").GetString();
        var shortCode = shortUrl!.Split('/').Last();

        // 1000 GET en paralelo
        var semaphore = new SemaphoreSlim(50, 50);
        var tasks = Enumerable.Range(0, 1000).Select(async _ =>
        {
            await semaphore.WaitAsync();
            try
            {
                var response = await _client.GetAsync($"/api/url/{shortCode}");
                Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            }
            finally { semaphore.Release(); }
        });

        await Task.WhenAll(tasks);
    }

}

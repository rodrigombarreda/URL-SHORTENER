using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

[Collection("IntegrationTests")]
public class CacheTests
{
    private readonly HttpClient _client;

    public CacheTests()
    {
        var handler = new HttpClientHandler { AllowAutoRedirect = false };
        _client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5000") };
        var token = JwtTestHelper.GenerateTestToken();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task Cache_ShouldSpeedUpRepeatedRequests()
    {
        // Crear una URL
        var json = "{\"longUrl\":\"https://example.com/cache-test\"}";
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/url/shorten", content);
        createResponse.EnsureSuccessStatusCode();

        var body = await createResponse.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var shortUrl = doc.RootElement.GetProperty("shortUrl").GetString();
        var shortCode = shortUrl!.Split('/').Last();

        // Primera request (SQL)
        var sw1 = System.Diagnostics.Stopwatch.StartNew();
        var firstResponse = await _client.GetAsync($"/api/url/{shortCode}");
        sw1.Stop();
        Assert.Equal(HttpStatusCode.Found, firstResponse.StatusCode);

        // 1000 requests en paralelo (Redis debería responder)
        var sw2 = System.Diagnostics.Stopwatch.StartNew();
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
        sw2.Stop();

        double avgBatchMs = (double)sw2.ElapsedMilliseconds / 1000;
        long firstRequestMs = Math.Max(sw1.ElapsedMilliseconds, 1);
        Assert.True(avgBatchMs < firstRequestMs,
            $"Cache no funcionó: primera={sw1.ElapsedMilliseconds}ms, promedio por request en batch={avgBatchMs:F2}ms");
    }
}

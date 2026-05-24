using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

[Collection("IntegrationTests")]
public class LoadTests
{
    private readonly HttpClient _client;

    public LoadTests()
    {
        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = false // 👈 evita que siga el redirect
        };

        _client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5000") };
        var token = JwtTestHelper.GenerateTestToken();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task Insert_1000ParallelRequests_ShouldSucceed()
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        var semaphore = new SemaphoreSlim(50, 50);
        var tasks = Enumerable.Range(0, 1000).Select(async i =>
        {
            await semaphore.WaitAsync();
            try
            {
                var json = $"{{\"longUrl\":\"https://example.com/load-insert-{i}\"}}";
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _client.PostAsync("/api/url/shorten", content);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
            finally { semaphore.Release(); }
        });

        await Task.WhenAll(tasks);
        sw.Stop();

        Console.WriteLine($"Insert 10.000 requests: {sw.ElapsedMilliseconds} ms");
    }

    [Fact]
    public async Task Get_1000ParallelRequests_ShouldSucceed()
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

        var sw = System.Diagnostics.Stopwatch.StartNew();

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
        sw.Stop();

        Console.WriteLine($"Get 10.000 requests: {sw.ElapsedMilliseconds} ms");
    }
}

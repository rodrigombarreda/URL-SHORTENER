using System.Net;
using System.Threading.Tasks;
using Xunit;

[Collection("IntegrationTests")]
public class MiddlewareTests
{
    private readonly HttpClient _client;

    public MiddlewareTests()
    {
        _client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5000") // 👈 puerto expuesto en docker-compose
        };

        var token = JwtTestHelper.GenerateTestToken();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task Middleware_ShouldReturnJsonError_OnException()
    {
        // Endpoint que sabés que lanza excepción (podés crear uno temporal en tu API para test)
        var response = await _client.GetAsync("/api/url/throw");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("message", body);
        Assert.Contains("Test exception", body);

    }
}

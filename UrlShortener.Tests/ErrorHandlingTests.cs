using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xunit;

[Collection("IntegrationTests")]
public class ErrorHandlingTests
{
    private readonly HttpClient _client;

    public ErrorHandlingTests()
    {
        _client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5000") // 👈 puerto expuesto en docker-compose
        };

        var token = JwtTestHelper.GenerateTestToken();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenMissingCredentials()
    {
        var json = "{}"; // faltan username y password
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/auth/login", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("message", body);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenInvalidCredentials()
    {
        var json = "{\"username\":\"wrong\",\"password\":\"bad\"}";
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/auth/login", content);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("message", body);
    }

    [Fact]
    public async Task ShortenUrl_ShouldReturnBadRequest_WhenLongUrlMissing()
    {
        var json = "{}"; // falta longUrl
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/url/shorten", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("message", body);
    }

    [Fact]
    public async Task GetLongUrl_ShouldReturnNotFound_WhenShortCodeDoesNotExist()
    {
        var response = await _client.GetAsync("/api/url/doesnotexist");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("message", body);
    }

    [Fact]
    public async Task GetLongUrl_ShouldReturnBadRequest_WhenShortCodeIsEmpty()
    {
        var response = await _client.GetAsync("/api/url/"); // endpoint sin código

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("message", body);
    }

    [Fact]
    public async Task ThrowError_ShouldReturnInternalServerError()
    {
        var response = await _client.GetAsync("/api/url/throw");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("message", body);
    }
}

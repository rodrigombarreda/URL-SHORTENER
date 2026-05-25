using Xunit;

// Pruebas de integración que verifican el comportamiento de deduplicación del sistema.
// El cache (Redis) y el unique constraint en DB garantizan que la misma longUrl
// siempre devuelve el mismo shortCode, incluso bajo carga concurrente.
// Requieren que la API esté corriendo en http://localhost:5000.
[Collection("IntegrationTests")]
public class CacheTests : IntegrationTestBase
{
    // Verifica que enviar la misma longUrl dos veces (secuencial) devuelve el mismo shortCode.
    // Prueba la deduplicación básica: el sistema no crea registros duplicados.
    [Fact]
    public async Task ShortenUrl_SameLongUrl_ShouldAlwaysReturnSameShortCode()
    {
        var longUrl = $"https://example.com/dedup-{Guid.NewGuid()}";

        var code1 = await CreateShortCode(longUrl);
        var code2 = await CreateShortCode(longUrl);

        Assert.Equal(code1, code2);
    }

    // Dispara 100 requests con la misma longUrl en simultáneo y verifica que todos
    // devuelvan exactamente el mismo shortCode. Prueba que el unique constraint en DB
    // y la transacción en CreateAsync previenen race conditions bajo carga concurrente.
    [Fact]
    public async Task ShortenUrl_SameLongUrl_100ConcurrentRequests_ShouldAllReturnSameShortCode()
    {
        var longUrl = $"https://example.com/race-{Guid.NewGuid()}";

        var tasks = Enumerable.Range(0, 100).Select(_ => CreateShortCode(longUrl));
        var results = await Task.WhenAll(tasks);

        Assert.Single(results.Distinct());
    }
}

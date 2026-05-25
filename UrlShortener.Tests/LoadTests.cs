using System.Net;
using Xunit;

// Pruebas de carga que verifican el comportamiento del sistema bajo alto volumen de requests.
// Miden la estabilidad y throughput con 1000 requests en paralelo (concurrencia de 50 a la vez).
// Los tiempos se imprimen en consola para poder compararlos entre ejecuciones.
// Requieren que la API esté corriendo en http://localhost:5000.
[Collection("IntegrationTests")]
public class LoadTests : IntegrationTestBase
{
    // Envía 1000 requests de creación en paralelo con URLs distintas.
    // Verifica que todos respondan 200 OK sin errores, demostrando
    // que la API maneja carga concurrente de escritura sin fallos.
    [Fact]
    public async Task Insert_1000ParallelRequests_ShouldAllSucceed()
    {
        var semaphore = new SemaphoreSlim(50, 50);
        var tasks = Enumerable.Range(0, 1000).Select(async i =>
        {
            await semaphore.WaitAsync();
            try
            {
                var response = await Client.PostAsync("/api/url/shorten",
                    JsonBody($"{{\"longUrl\":\"https://example.com/load-insert-{i}\"}}"));
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
            finally { semaphore.Release(); }
        });

        var sw = System.Diagnostics.Stopwatch.StartNew();
        await Task.WhenAll(tasks);
        sw.Stop();

        Console.WriteLine($"Insert 1000 parallel requests: {sw.ElapsedMilliseconds}ms");
    }

    // Envía 1000 requests de redirect en paralelo sobre el mismo shortCode.
    // Verifica que todos respondan 302 Found, probando que el cache de Redis
    // soporta alto volumen de lecturas concurrentes sin degradar.
    [Fact]
    public async Task Get_1000ParallelRequests_ShouldAllRedirect()
    {
        var shortCode = await CreateShortCode("https://example.com/load-get");

        var semaphore = new SemaphoreSlim(50, 50);
        var tasks = Enumerable.Range(0, 1000).Select(async _ =>
        {
            await semaphore.WaitAsync();
            try
            {
                var response = await Client.GetAsync($"/api/url/{shortCode}");
                Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            }
            finally { semaphore.Release(); }
        });

        var sw = System.Diagnostics.Stopwatch.StartNew();
        await Task.WhenAll(tasks);
        sw.Stop();

        Console.WriteLine($"Get 1000 parallel requests: {sw.ElapsedMilliseconds}ms");
    }
}

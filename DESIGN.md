# URL Shortener - Design Document

## 1. Arquitectura

### Clean Architecture (4 capas)

El sistema sigue Clean Architecture para asegurar separación de responsabilidades y facilidad de mantenimiento:

```
UrlShortener.Core          → Dominio puro, sin dependencias externas
UrlShortener.Application   → Orquestación de lógica de negocio y DTOs
UrlShortener.Infrastructure → EF Core, Redis, acceso a datos
UrlShortener.Api           → Controladores, middleware, HTTP
UrlShortener.Tests         → Tests unitarios, de integración y carga
```

#### Core
- Entidad principal: `UrlTable` (Id, LongUrl, ShortCode, CreatedOnUtc)
- Interfaces de contrato: `IUrlService`, `IUrlRepository`, `ICacheService`
- Sin dependencias externas.

#### Application
- `UrlService`: orquesta creación y resolución de URLs
  - Consulta primero Redis, hace fallback a SQL si cache miss
  - Registra métricas Prometheus (`urls_created_total`, `urls_redirected_total`)
- `RedisCacheService`: implementa `ICacheService` con TTL de 10 minutos y degradación graceful ante fallos
- DTOs: `CreateShortenUrlRequest`, `CreateShortenUrlResponse`, `LoginRequest`

#### Infrastructure
- `UrlRepository` (EF Core): inserta la URL dentro de una transacción, genera el shortCode Base62 a partir del ID autogenerado, y maneja violaciones de unique constraint (race conditions)
- `UrlShortenerDbContext`: índice único en `ShortCode` (case-sensitive, collation `Latin1_General_100_BIN2`) e índice único en `LongUrl` (garantía de deduplicación a nivel DB)
- `Base62Converter`: codifica IDs enteros a base62 usando `Span<char>` en stack (zero heap allocations)
- 5 migraciones EF Core (InitialCreate → AddIndexes → MakeShortCodeNullable → MakeShortCodeCaseSensitive → AddUniqueLongUrlConstraint)

#### Api
- `AuthController` POST `/api/auth/login` → devuelve JWT (exp: 60 min)
- `UrlController`:
  - POST `/api/url/shorten` → crea URL corta, requiere JWT
  - GET `/api/url/{shortCode}` → HTTP 302 redirect al original, público (`[AllowAnonymous]`)
- `ErrorHandlingMiddleware`: captura todas las excepciones y responde con `{ code, message, timestamp }`

---

## 2. Stack Tecnológico

| Componente      | Tecnología                                      |
|-----------------|-------------------------------------------------|
| Runtime         | .NET 10.0 / ASP.NET Core                        |
| Base de datos   | SQL Server 2022 (EF Core 10)                    |
| Caché           | Redis (StackExchange.Redis 2.13)                |
| Autenticación   | JWT Bearer (Microsoft.AspNetCore.Authentication)|
| Métricas        | Prometheus (`prometheus-net.AspNetCore`)        |
| Contenedores    | Docker Compose (API + SQL Server + Redis)       |
| Tests           | xUnit 2.9.3 + coverlet 6.0.4                   |

---

## 3. Manejo de Errores

El `ErrorHandlingMiddleware` mapea excepciones a respuestas HTTP:

| Excepción                  | HTTP Status          |
|----------------------------|----------------------|
| `ArgumentException`        | 400 Bad Request      |
| `KeyNotFoundException`     | 404 Not Found        |
| `UnauthorizedAccessException` | 401 Unauthorized  |
| Cualquier otra             | 500 Internal Server Error |

Formato de respuesta siempre: `{ "code": 4xx/5xx, "message": "...", "timestamp": "..." }`

---

## 4. Seguridad

- JWT con claims de nombre y rol (`User`) firmado con clave simétrica HS256
- El endpoint de creación (`POST /shorten`) requiere JWT; el redirect (`GET /{shortCode}`) es público por diseño
- HTTPS habilitado para producción
- Collation case-sensitive en `ShortCode` para evitar colisiones silenciosas (`abc` ≠ `ABC`)

---

## 5. Observabilidad

- **Prometheus**: métricas expuestas en `/metrics`
  - `urls_created_total`: contador de URLs acortadas creadas
  - `urls_redirected_total`: contador de redirecciones resueltas
- **Structured logging**: `ILogger<T>` en todas las capas

---

## 6. Caching

- Redis como caché distribuido con TTL de 10 minutos por entrada
- Fallback automático a SQL si Redis no está disponible: `GetAsync` retorna null, `SetAsync` loguea el error y continúa — el cache es una optimización, no un hard dependency
- Dos claves de caché por URL, para poder buscar en ambas direcciones:
  - `url:<shortCode>` → longUrl (usada por el flujo de redirect)
  - `longurl:<longUrl>` → shortCode (usada por el flujo de creación para deduplicar)
- El cache se popula en la creación (`CreateShortUrlAsync`), no solo en el primer redirect
- Flujo de `CreateShortUrlAsync`: check cache → check DB → crear; en cada paso se populan ambas keys

---

## 7. Testing

| Archivo                      | Tipo        | Descripción                                                                 |
|------------------------------|-------------|-----------------------------------------------------------------------------|
| `IntegrationTestBase.cs`     | Base        | Setup compartido: HttpClient autenticado, sin auth, y helpers reutilizables |
| `UrlControllerTests.cs`      | Integración | Happy path (crear + redirigir) y comportamiento de autenticación            |
| `CacheTests.cs`              | Integración | Deduplicación secuencial y bajo 100 requests concurrentes (race condition)  |
| `ErrorHandlingTests.cs`      | Integración | Status codes y mensajes ante inputs inválidos y recursos inexistentes       |
| `MiddlewareTests.cs`         | Integración | Estructura JSON completa del error (`code`, `message`, `timestamp`, Content-Type) |
| `LoadTests.cs`               | Carga       | 1000 requests paralelos de escritura y lectura; mide throughput             |
| `IntegrationTestsCollection.cs` | Config   | Deshabilita paralelismo entre clases para evitar conflictos en DB y Redis   |
| `JwtTestHelper.cs`           | Helper      | Genera tokens JWT válidos para los tests                                    |

---

## 8. Deployment

```yaml
# docker-compose.yml
services:
  api:      # UrlShortener.Api — puerto 5000 → 8080
  sqlserver: # SQL Server 2022 — puerto 1433
  redis:    # Redis latest — puerto 6379
```

Levantar todo el sistema: `docker compose up --build`

---

## 9. Uso de AI

Durante el desarrollo se usaron herramientas de AI (GitHub Copilot, Claude Code) para:

- Generar código base de controladores, servicios y repositorios
- Proponer patrones de manejo de errores y logging estructurado
- Sugerir estructura de carpetas bajo Clean Architecture
- Escribir y refinar tests de integración y carga

**Criterio aplicado**: todo código generado por AI fue revisado, adaptado y validado manualmente. Se priorizó mantener el código limpio, consistente y cubierto por tests.

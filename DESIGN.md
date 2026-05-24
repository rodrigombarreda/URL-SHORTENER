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
- `UrlRepository` (EF Core): inserta la URL, genera el shortCode Base62 a partir del ID autogenerado, y actualiza el registro
- `UrlShortenerDbContext`: configura índice único en `ShortCode` con collation `Latin1_General_100_BIN2` (case-sensitive) e índice en `LongUrl`
- `Base62Converter`: codifica IDs enteros a base62 (0-9, a-z, A-Z), produciendo códigos cortos, legibles y URL-safe
- 4 migraciones EF Core (InitialCreate → AddIndexes → MakeShortCodeNullable → MakeShortCodeCaseSensitive)

#### Api
- `AuthController` POST `/api/auth/login` → devuelve JWT (exp: 60 min)
- `UrlController`:
  - POST `/api/url/shorten` → crea URL corta, requiere JWT
  - GET `/api/url/{shortCode}` → HTTP 302 redirect al original, requiere JWT
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
| Observabilidad  | OpenTelemetry 1.15                              |
| Contenedores    | Docker Compose (API + SQL Server + Redis)       |
| Tests           | xUnit 2.9.3 + coverlet                         |

---

## 3. Trade-offs

- **Base62 vs GUID**: Base62 produce códigos compactos y legibles (`G8` en lugar de un UUID). Los GUIDs serían más únicos globalmente pero no son URL-friendly.
- **Redis vs InMemoryCache**: Redis permite escalar horizontalmente con múltiples instancias de API. InMemory sería más simple pero los cachés no se compartirían entre instancias.
- **EF Core vs Dapper**: EF Core facilita las migraciones y consultas con LINQ. Dapper tendría mejor throughput puro, pero más código manual.
- **Middleware global vs try/catch en controllers**: El middleware centraliza el manejo de errores y evita duplicación. Permite un formato de respuesta uniforme sin tocar cada controller.
- **Generación del shortCode en dos pasos**: Se inserta primero la entidad para obtener el ID autogenerado y luego se codifica en Base62. Esto garantiza unicidad sin lógica adicional de colisiones.

---

## 4. Manejo de Errores

El `ErrorHandlingMiddleware` mapea excepciones a respuestas HTTP:

| Excepción                  | HTTP Status          |
|----------------------------|----------------------|
| `ArgumentException`        | 400 Bad Request      |
| `KeyNotFoundException`     | 404 Not Found        |
| `UnauthorizedAccessException` | 401 Unauthorized  |
| Cualquier otra             | 500 Internal Server Error |

Formato de respuesta siempre: `{ "code": 4xx/5xx, "message": "...", "timestamp": "..." }`

---

## 5. Seguridad

- JWT con claims de nombre y rol (`User`) firmado con clave simétrica HS256
- Todos los endpoints de `UrlController` requieren autenticación
- HTTPS habilitado para producción
- Collation case-sensitive en `ShortCode` para evitar colisiones silenciosas (`abc` ≠ `ABC`)

---

## 6. Observabilidad

- **Prometheus**: métricas expuestas en `/metrics`
  - `urls_created_total`: contador de URLs acortadas creadas
  - `urls_redirected_total`: contador de redirecciones resueltas
- **OpenTelemetry**: trazas distribuidas configuradas
- **Structured logging**: `ILogger<T>` en todas las capas

---

## 7. Caching

- Redis como caché distribuido con TTL de 10 minutos por entrada
- Fallback automático a SQL si Redis no está disponible (timeout o conexión rechazada)
- La clave de caché es el `shortCode`; el valor es el `longUrl`

---

## 8. Testing

| Archivo                    | Tipo        | Descripción                                             |
|----------------------------|-------------|---------------------------------------------------------|
| `UrlControllerTests.cs`    | Integración | POST/GET de endpoints, tests de estrés paralelos (1000 requests) |
| `CacheTests.cs`            | Integración | Verifica que Redis acelera requests repetidos           |
| `ErrorHandlingTests.cs`    | Unitario    | Formato y status codes de respuestas de error           |
| `MiddlewareTests.cs`       | Unitario    | Comportamiento del `ErrorHandlingMiddleware`            |
| `LoadTests.cs`             | Carga       | Benchmarking y medición de performance bajo carga       |
| `JwtTestHelper.cs`         | Helper      | Genera tokens JWT válidos para los tests                |

---

## 9. Deployment

```yaml
# docker-compose.yml
services:
  api:      # UrlShortener.Api — puerto 5000 → 8080
  sqlserver: # SQL Server 2022 — puerto 1433
  redis:    # Redis latest — puerto 6379
```

Levantar todo el sistema: `docker compose up --build`

---

## 10. Uso de AI

Durante el desarrollo se usaron herramientas de AI (GitHub Copilot, Claude Code) para:

- Generar código base de controladores, servicios y repositorios
- Proponer patrones de manejo de errores y logging estructurado
- Sugerir estructura de carpetas bajo Clean Architecture
- Escribir y refinar tests de integración y carga

**Criterio aplicado**: todo código generado por AI fue revisado, adaptado y validado manualmente. Se priorizó mantener el código limpio, consistente y cubierto por tests.

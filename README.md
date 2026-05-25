# URL Shortener

API REST para acortar URLs, construida con ASP.NET Core, SQL Server y Redis.

---

## Requisitos previos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) (requerido para correr los tests localmente)

---

## Levantar la aplicación

```bash
docker compose up --build
```

Esto levanta tres contenedores:
- **API** → http://localhost:5000
- **SQL Server** → localhost:1433
- **Redis** → localhost:6379

Las migraciones de base de datos se aplican automáticamente al iniciar la API.

Para detenerlo:
```bash
docker compose down
```

---

## Usar la API

### 1. Obtener un token JWT

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "test", "password": "1234"}'
```

Respuesta:
```json
{ "token": "eyJ..." }
```

> Las credenciales hardcodeadas son `test` / `1234` (ambiente de prueba).

---

### 2. Acortar una URL

```bash
curl -X POST http://localhost:5000/api/url/shorten \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"longUrl": "https://www.google.com/search?q=url+shortener"}'
```

Respuesta:
```json
{ "shortUrl": "http://localhost:5000/api/url/G8" }
```

---

### 3. Resolver una URL corta

```bash
curl -L -X GET http://localhost:5000/api/url/G8
```

Devuelve HTTP **302** con el header `Location` apuntando a la URL original.  
Con `-L`, `curl` sigue el redirect automáticamente. Este endpoint es público — no requiere token.

---

### Métricas Prometheus

```
GET http://localhost:5000/metrics
```

Métricas disponibles:
- `urls_created_total` — total de URLs acortadas creadas
- `urls_redirected_total` — total de redirecciones resueltas
- Métricas HTTP automáticas de ASP.NET Core

---

## Correr los tests

> Los tests de integración y carga requieren que la API esté corriendo en `http://localhost:5000`.

### Levantar la API para los tests (si no está corriendo)

```bash
docker compose up -d
```

### Correr todos los tests

Desde el directorio `UrlShortener.Tests/`:

```bash
cd UrlShortener.Tests
dotnet test
```

### Correr con detalle de resultados

```bash
dotnet test --logger "console;verbosity=normal"
```

### Correr con reporte de cobertura

```bash
dotnet test --collect:"XPlat Code Coverage"
```

El reporte de cobertura se genera en `UrlShortener.Tests/TestResults/`.

---

## Estructura del proyecto

```
URL-SHORTENER/
├── UrlShortener.Api/           # Controladores, middleware, Program.cs
├── UrlShortener.Application/   # Servicios y DTOs de negocio
├── UrlShortener.Core/          # Entidades e interfaces (dominio puro)
├── UrlShortener.Infrastructure/# EF Core, Redis, repositorios, migraciones
├── UrlShortener.Tests/         # Tests unitarios, integración y carga
├── docker-compose.yml
└── DESIGN.md                   # Decisiones de arquitectura y trade-offs
```

---

## Resumen de endpoints

| Método | Endpoint                 | Auth | Descripción                     |
|--------|--------------------------|------|---------------------------------|
| POST   | `/api/auth/login`        | No   | Obtiene token JWT               |
| POST   | `/api/url/shorten`       | JWT  | Acorta una URL larga            |
| GET    | `/api/url/{shortCode}`   | No   | Redirige (302) a la URL original |
| GET    | `/metrics`               | No   | Métricas Prometheus             |


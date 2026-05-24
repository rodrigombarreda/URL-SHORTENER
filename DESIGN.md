# URL Shortener - Design Document

## 1. Architectural Choices

### Clean Architecture

El sistema sigue una arquitectura en capas (Clean Architecture) para asegurar separación de responsabilidades y facilidad de mantenimiento:

- **Core**  
  Contiene entidades (`UrlTable`), DTOs, interfaces y excepciones de negocio.  
  → Mantiene el dominio puro y sin dependencias externas.

- **Application**  
  Contiene servicios (`UrlService`) que orquestan la lógica de negocio.  
  → Usa interfaces de Core y delega persistencia/caché a Infrastructure.

- **Infrastructure**  
  Contiene implementaciones técnicas:
  - `UrlRepository` (EF Core para SQL)
  - `RedisCacheService` (StackExchange.Redis)  
    → Encapsula detalles de almacenamiento y caché.

- **Api**  
  Contiene controladores (`AuthController`, `UrlController`) y middleware global de errores.  
  → Expone endpoints REST y traduce excepciones en respuestas HTTP limpias.

### Error Handling

- Middleware global (`ErrorHandlingMiddleware`) que captura todas las excepciones y devuelve un JSON uniforme `{ code, message, timestamp }`.
- Excepciones personalizadas en Core (`UrlNotFoundException`, `DuplicateUrlException`).
- Logging en cada capa para trazabilidad.

### Security

- Autenticación con JWT en `AuthController`.
- Roles básicos (`User`) para proteger endpoints.

### Caching

- Redis como caché distribuido para mejorar performance en redirecciones.
- Fallback seguro: si Redis falla, se consulta SQL.

---

## 2. Trade-offs

- **Base62 vs GUID**  
  Se eligió Base62 para generar shortCodes legibles y compactos. GUIDs serían más robustos pero menos amigables.
- **Redis vs InMemoryCache**  
  Redis permite escalabilidad horizontal. InMemoryCache sería más simple pero no soporta múltiples instancias.
- **Middleware global vs try/catch en controllers**  
  Se prefirió middleware global para centralizar manejo de errores y evitar duplicación de lógica.
- **EF Core vs Dapper**  
  EF Core facilita el desarrollo y pruebas con LINQ y migraciones. Dapper sería más rápido pero menos expresivo.

---

## 3. Use of AI

Durante el desarrollo se usaron herramientas de AI (Copilot, ClaudeCode, etc.) para:

- Generar código base de controladores y servicios.
- Proponer patrones de manejo de errores y logging.
- Sugerir estructura de carpetas y aplicación de Clean Architecture.
- Documentar funciones y clases con comentarios claros.

**Decisión consciente:**

- Todo código generado por AI fue revisado, adaptado y comentado para asegurar comprensión total.
- Se evitó “AI slop” manteniendo el código limpio, consistente y con tests unitarios.

---

## 4. Testing

- Tests unitarios para `UrlService` (crear y obtener URLs).
- Tests para `Base62Converter`.
- Tests de integración para `UrlController` con un `TestServer`.
- Tests de middleware para verificar formato de errores.

---

## 5. Deployment

- `docker-compose.yml` con API + SQL Server + Redis.
- Facilita levantar todo el sistema con un solo comando.
- Preparado para entrevistas y entornos de prueba.

---

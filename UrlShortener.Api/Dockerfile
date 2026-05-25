# ── Etapa 1: build ──────────────────────────────────────────────────────────
# Usa la imagen del SDK de .NET 10 (incluye compilador, NuGet, etc.)
# Solo se usa para compilar; no llega a la imagen final.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

# Directorio de trabajo dentro del contenedor durante la compilación
WORKDIR /src

# Copia todo el repositorio al contenedor
COPY . .

# Entra al proyecto de la API para ejecutar los comandos de .NET
WORKDIR /src/UrlShortener.Api

# Restaura los paquetes NuGet definidos en el .csproj
RUN dotnet restore UrlShortener.Api.csproj

# Compila en modo Release y publica los binarios en /app/publish
RUN dotnet publish -c Release -o /app/publish


# ── Etapa 2: runtime ─────────────────────────────────────────────────────────
# Usa la imagen del runtime de ASP.NET 10 (más liviana, sin SDK ni compilador)
# Solo copia los binarios ya compilados de la etapa anterior.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

# Copia únicamente los artefactos publicados desde la etapa "build"
COPY --from=build /app/publish .

# Comando que se ejecuta al iniciar el contenedor
ENTRYPOINT ["dotnet", "UrlShortener.Api.dll"]

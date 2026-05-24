FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copiar todo el repo
COPY . .

WORKDIR /src/UrlShortener.Api
RUN dotnet restore UrlShortener.Api.csproj
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "UrlShortener.Api.dll"]

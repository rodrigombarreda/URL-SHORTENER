using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UrlShortener.Infrastructure.Extensions;
using UrlShortener.Application.Extensions;
using UrlShortener.Infrastructure.Data;
using StackExchange.Redis;
using Prometheus; // 👈 importante

var builder = WebApplication.CreateBuilder(args);

// Configuración JWT
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization();
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var firstError = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .SelectMany(e => e.Value!.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault() ?? "Validation failed.";

            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(new { message = firstError });
        };
    });
builder.Services.AddOpenApi();

// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(
   ConnectionMultiplexer.Connect(builder.Configuration["ConnectionStrings:Redis"] ?? "redis:6379"));

// Servicios propios
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Middleware de errores global
app.UseMiddleware<UrlShortener.Api.Middleware.ErrorHandlingMiddleware>();

// Autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

// Prometheus: expone /metrics y mide requests HTTP
app.UseMetricServer();   // Endpoint /metrics
app.UseHttpMetrics();    // Métricas automáticas de requests

// Controllers
app.MapControllers();

// Migraciones automáticas
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UrlShortenerDbContext>();
    db.Database.Migrate();
}

app.Run();

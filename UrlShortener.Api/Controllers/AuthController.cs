using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UrlShortener.Core.DTOs;

namespace UrlShortener.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IConfiguration config, ILogger<AuthController> logger)
        {
            _config = config;
            _logger = logger;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Username and password are required." });
            }

            var username = request.Username;
            var password = request.Password;

            if (username == "test" && password == "1234")
            {
                var jwtSettings = _config.GetSection("Jwt");
                var keyValue = jwtSettings["Key"];

                if (string.IsNullOrEmpty(keyValue))
                {
                    throw new ApplicationException("JWT Key is missing in configuration.");
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyValue));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, "User")
            };

                var token = new JwtSecurityToken(
                    issuer: jwtSettings["Issuer"],
                    audience: jwtSettings["Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpiresInMinutes"] ?? "30")),
                    signingCredentials: creds
                );

                _logger.LogInformation("User {Username} logged in successfully.", username);

                return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
            }

            _logger.LogWarning("Invalid login attempt for user {Username}", username);
            return Unauthorized(new { message = "Invalid credentials." });
        }
    }

}
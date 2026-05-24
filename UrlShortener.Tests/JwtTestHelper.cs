using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

public static class JwtTestHelper
{
    public static string GenerateTestToken()
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("EstaEsUnaClaveSuperSeguraDeAlMenos32Caracteres")); // debe ser la misma que uses en la API

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "UrlShortener",
            audience: "UrlShortenerUsers",
            claims: new[] { new Claim(ClaimTypes.Name, "testuser") },
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

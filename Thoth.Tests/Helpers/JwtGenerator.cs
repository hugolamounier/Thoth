using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Thoth.Sample;

namespace Thoth.Tests.Helpers;

public static class JwtGenerator
{
    public static string GenerateToken(
        IEnumerable<Claim> claims, int daysToExpire = 1,
        string? audience = null,
        string? issuer = null,
        SigningCredentials? signingCredentials = null)
    {

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Audience = audience ?? JwtConfiguration.Audience,
            Issuer = issuer ?? JwtConfiguration.Issuer,
            Expires = DateTime.UtcNow.AddDays(daysToExpire),
            SigningCredentials = signingCredentials ?? new SigningCredentials
            (
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtConfiguration.HmacKey)),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public static IEnumerable<Claim> ReadClaimsFromStringToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var paramsValidation = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            IssuerSigningKey = new SymmetricSecurityKey
            (
                Encoding.UTF8.GetBytes(JwtConfiguration.HmacKey)
            ),
            ValidAudience = JwtConfiguration.Audience,
            ValidIssuer = JwtConfiguration.Issuer
        };

        var principal = tokenHandler.ValidateToken(token, paramsValidation, out _);

        return principal.Claims;
    }
}
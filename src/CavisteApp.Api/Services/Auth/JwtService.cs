using System.Text;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using CavisteApp.Api.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace CavisteApp.Api.Services.Auth;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(ApplicationUser utilisateur, IList<string> roles, out DateTime expiration)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var secretKey = jwtSection["SecretKey"] ?? throw new InvalidOperationException("SecretKey is not configured in appsettings.json");
        var issuer = jwtSection["Issuer"] ?? throw new InvalidOperationException("Issuer is not configured in appsettings.json");
        var audience = jwtSection["Audience"] ?? throw new InvalidOperationException("Audience is not configured in appsettings.json");
        var expirationMinutes = int.Parse(jwtSection["ExpirationMinutes"] ?? throw new InvalidOperationException("ExpirationMinutes is not configured in appsettings.json"));

        expiration = DateTime.UtcNow.AddMinutes(expirationMinutes);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, utilisateur.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, utilisateur.UserName ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiration,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

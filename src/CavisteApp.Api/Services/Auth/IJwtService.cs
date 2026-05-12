using CavisteApp.Api.Entities;

namespace CavisteApp.Api.Services.Auth;

public interface IJwtService
{
    string GenerateToken(ApplicationUser utilisateur, IList<string> roles, out DateTime expiration);
}

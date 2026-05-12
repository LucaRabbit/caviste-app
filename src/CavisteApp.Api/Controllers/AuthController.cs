using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using CavisteApp.Api.Entities;
using CavisteApp.Api.Services.Auth;
using CavisteApp.DTOs.Auth;

namespace CavisteApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtService _jwtService;

    public AuthController(UserManager<ApplicationUser> userManager, IJwtService jwtService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Unauthorized(new { message = "Login ou mot de passe incorrect." });
        }

        var user = await _userManager.FindByNameAsync(request.Login);
        if (user is null)
        {
            return Unauthorized(new { message = "Login ou mot de passe incorrect." });
        }

        var passwordValidated = await _userManager.CheckPasswordAsync(user, request.Password);
        if (passwordValidated is false)
        {
            return Unauthorized(new { message = "Login ou mot de passe incorrect." });
        }

        var roles = await _userManager.GetRolesAsync(user);

        var token = _jwtService.GenerateToken(user, roles, out var expiration);

        return Ok(new LoginResponse
        {
            Token = token,
            Expiration = DateTime.UtcNow.AddHours(1),
            Utilisateur = new UtilisateurDto
            {
                Id = user.Id,
                Login = user.UserName ?? string.Empty,
                Role = roles.FirstOrDefault() ?? "visiteur"
            }
        });
    }
}
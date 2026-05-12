using Microsoft.AspNetCore.Identity;
using CavisteApp.Api.Entities;
using CavisteApp.Api.Constants;

namespace CavisteApp.Api.Services.Auth;

public class AuthSeeder
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;

    public AuthSeeder(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<int>> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task SeedAsync()
    {
        await CreateRoleIfAbsentAsync(RolesConstants.Administrateur);
        await CreateRoleIfAbsentAsync(RolesConstants.Visiteur);

        await CreateUserIfAbsentAsync(
            login: "admin",
            email: "admin@caviste.local",
            password: "admin123",
            role: RolesConstants.Administrateur);

        await CreateUserIfAbsentAsync(
            login: "visiteur",
            email: "visiteur@caviste.local",
            password: "visiteur123",
            role: RolesConstants.Visiteur);
    }

    private async Task CreateRoleIfAbsentAsync(string nomRole)
    {
        if (!await _roleManager.RoleExistsAsync(nomRole))
        {
            await _roleManager.CreateAsync(new IdentityRole<int>(nomRole));
        }
    }

    private async Task CreateUserIfAbsentAsync(string login, string email, string password, string role)
    {
        if (await _userManager.FindByNameAsync(login) is not null) return;

        var user = new ApplicationUser
        {
            UserName = login,
            Email = email
        };

        var result = await _userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, role);
        }
    }
}

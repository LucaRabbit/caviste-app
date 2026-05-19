using Microsoft.AspNetCore.Identity;

namespace CavisteApp.Api.Entities;

public class ApplicationUser : IdentityUser<int>
{
    public DateTime DateCreation { get; set; } = DateTime.Now;
}

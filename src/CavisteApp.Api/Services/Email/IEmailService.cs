// CavisteApp.Api/Services/Email/IEmailService.cs
namespace CavisteApp.Api.Services.Email;

public interface IEmailService
{
    Task EnvoyerAsync(string sujet, string corpsHtml, CancellationToken ct = default);
}
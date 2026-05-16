using CavisteApp.Api.Services.Email;
using Microsoft.AspNetCore.Mvc;

[ApiController, Route("api/[controller]")]
public class TestEmailController : ControllerBase
{
    private readonly IEmailService _email;
    public TestEmailController(IEmailService email) => _email = email;

    [HttpPost("envoyer")]
    public async Task<IActionResult> Envoyer(CancellationToken ct)
    {
        await _email.EnvoyerAsync("Test Mailtrap", "<h1>Ça marche 🍷</h1>", ct);
        return Ok(new { message = "Mail envoyé (vérifier la sandbox Mailtrap)" });
    }
}
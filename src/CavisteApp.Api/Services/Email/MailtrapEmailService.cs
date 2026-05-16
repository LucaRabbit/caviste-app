// CavisteApp.Api/Services/Email/MailtrapEmailService.cs
using CavisteApp.Api.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace CavisteApp.Api.Services.Email;

public class MailtrapEmailService : IEmailService
{
    private readonly MailtrapOptions _opt;
    private readonly ILogger<MailtrapEmailService> _log;

    public MailtrapEmailService(IOptions<MailtrapOptions> opt, ILogger<MailtrapEmailService> log)
    {
        _opt = opt.Value;
        _log = log;
    }

    public async Task EnvoyerAsync(string sujet, string corpsHtml, CancellationToken ct = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_opt.FromName, _opt.FromEmail));
        message.To.Add(MailboxAddress.Parse(_opt.ToEmail));
        message.Subject = sujet;
        message.Body = new TextPart("html") { Text = corpsHtml };

        try
        {
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_opt.Host, _opt.Port, SecureSocketOptions.StartTls, ct);
            await smtp.AuthenticateAsync(_opt.Username, _opt.Password, ct);
            await smtp.SendAsync(message, ct);
            await smtp.DisconnectAsync(true, ct);

            _log.LogInformation("Mail envoyé : {Sujet}", sujet);
        }
        catch (Exception ex)
        {
            // Important : on ne laisse PAS l'exception remonter, sinon une panne SMTP
            // ferait planter la vente. Le mail est une notification, pas une opération critique.
            _log.LogError(ex, "Échec d'envoi du mail : {Sujet}", sujet);
        }
    }
}
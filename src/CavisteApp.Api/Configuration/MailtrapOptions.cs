// CavisteApp.Api/Configuration/MailtrapOptions.cs
namespace CavisteApp.Api.Configuration;

public class MailtrapOptions
{
    public string Host { get; set; } = "";
    public int Port { get; set; } = 2525;
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string FromEmail { get; set; } = "";
    public string FromName { get; set; } = "";
    public string ToEmail { get; set; } = "";
}
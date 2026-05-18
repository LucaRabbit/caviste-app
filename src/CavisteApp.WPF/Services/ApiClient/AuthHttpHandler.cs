using System.Net.Http;
using System.Net.Http.Headers;

namespace CavisteApp.WPF.Services.ApiClient;

internal class AuthHttpHandler : DelegatingHandler
{
    private readonly SessionService _session;

    public AuthHttpHandler(SessionService session)
    {
        _session = session;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(_session.Token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _session.Token);
        }
        return base.SendAsync(request, cancellationToken);
    }
}

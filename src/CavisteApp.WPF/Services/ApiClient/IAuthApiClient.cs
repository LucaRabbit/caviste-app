using CavisteApp.DTOs.Auth;

namespace CavisteApp.WPF.Services.ApiClient
{
    public interface IAuthApiClient
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    }
}

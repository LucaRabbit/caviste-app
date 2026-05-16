using System.Net;
using System.Text.Json;
using CavisteApp.Api.ExternalApi;

public interface IWineApiClient
{
    Task<WineSearchResponse> SearchAsync(string query, int limit = 20, CancellationToken ct = default);
    Task<WineDetailsDto?> GetDetailsAsync(string wineId, CancellationToken ct = default);
}
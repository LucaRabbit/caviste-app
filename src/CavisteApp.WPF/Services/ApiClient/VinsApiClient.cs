using System.Net.Http;
using System.Net.Http.Json;
using CavisteApp.DTOs.Vins;

namespace CavisteApp.WPF.Services.ApiClient;
// Implémentation du client API pour les opérations sur les vins, utilisant HttpClient pour communiquer avec l'API REST
public class VinsApiClient : IVinsApiClient
{
    private readonly HttpClient _http;

    public VinsApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<VinDto>> GetTousAsync(CancellationToken ct = default)
    {
        var vins = await _http.GetFromJsonAsync<List<VinDto>>("api/vins", JsonOptions.Default, ct);
        return vins ?? new List<VinDto>();
    }

    public async Task<VinDto?> GetParIdAsync(int id, CancellationToken ct = default)
    {
        var response = await _http.GetAsync($"api/vins/{id}", ct);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<VinDto>(JsonOptions.Default, ct);
    }

    public async Task<VinDto> CreerAsync(CreerVinDto request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/vins", request, JsonOptions.Default, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<VinDto>(JsonOptions.Default, ct))!;
    }

    public async Task ModifierAsync(int id, UpdateVinDto request, CancellationToken ct = default)
    {
        var response = await _http.PutAsJsonAsync($"api/vins/{id}", request, JsonOptions.Default, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task AjusterStockAsync(int id, InventaireDto request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync($"api/vins/{id}/inventaire", request, JsonOptions.Default, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task SupprimerAsync(int id, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync($"api/vins/{id}", ct);
        response.EnsureSuccessStatusCode();
    }
}
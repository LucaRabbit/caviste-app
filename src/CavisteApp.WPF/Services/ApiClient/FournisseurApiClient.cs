using CavisteApp.DTOs.Fournisseurs;
using CavisteApp.DTOs.Vins;
using System.Net.Http;
using System.Net.Http.Json;

namespace CavisteApp.WPF.Services.ApiClient;
// Implémentation du client API pour les opérations sur les fournisseurs, utilisant HttpClient pour communiquer avec l'API REST
public class FournisseursApiClient : IFournisseursApiClient
{
    private readonly HttpClient _http;

    public FournisseursApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<FournisseurDto>> GetTousAsync(CancellationToken ct = default)
    {
        var fournisseurs = await _http.GetFromJsonAsync<List<FournisseurDto>>("api/fournisseurs", JsonOptions.Default, ct);
        return fournisseurs ?? new List<FournisseurDto>();
    }

    public async Task<FournisseurDto?> GetParIdAsync(int id, CancellationToken ct = default)
    {
        var response = await _http.GetAsync($"api/fournisseurs/{id}", ct);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<FournisseurDto>(JsonOptions.Default, ct);
    }

    public async Task<FournisseurDto> CreerAsync(CreerFournisseurDto request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/fournisseurs", request, JsonOptions.Default, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<FournisseurDto>(JsonOptions.Default, ct))!;
    }

    public async Task ModifierAsync(int id, UpdateFournisseurDto request, CancellationToken ct = default)
    {
        var response = await _http.PutAsJsonAsync($"api/fournisseurs/{id}", request, JsonOptions.Default, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task AjusterStockAsync(int id, InventaireDto request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync($"api/fournisseurs/{id}/inventaire", request, JsonOptions.Default, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task SupprimerAsync(int id, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync($"api/fournisseurs/{id}", ct);
        response.EnsureSuccessStatusCode();
    }
}
using System.Net.Http;
using System.Net.Http.Json;
using CavisteApp.Api.Dtos.Ventes;
using CavisteApp.DTOs.Enums;
using CavisteApp.DTOs.Ventes;

namespace CavisteApp.WPF.Services.ApiClient;

public class VentesApiClient : IVentesApiClient
{
    private readonly HttpClient _http;
    public VentesApiClient(HttpClient http) => _http = http;

    public async Task<List<VenteResumeDto>> GetTousAsync(StatutVente? statut = null, int? clientId = null, CancellationToken ct = default)
    {
        var url = "api/ventes";
        var qs = new List<string>();
        if (statut.HasValue) qs.Add($"statut={statut.Value}");
        if (clientId.HasValue) qs.Add($"clientId={clientId.Value}");
        if (qs.Count > 0) url += "?" + string.Join("&", qs);

        var ventes = await _http.GetFromJsonAsync<List<VenteResumeDto>>(url, JsonOptions.Default, ct);
        return ventes ?? new List<VenteResumeDto>();
    }

    public async Task<VenteDto?> GetParIdAsync(int id, CancellationToken ct = default)
    {
        var response = await _http.GetAsync($"api/ventes/{id}", ct);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();

        // DEBUG temporaire
        var json = await response.Content.ReadAsStringAsync(ct);
        System.Diagnostics.Debug.WriteLine($"[GetParIdAsync] {json}");

        return System.Text.Json.JsonSerializer.Deserialize<VenteDto>(json, JsonOptions.Default);
    }

    public async Task<VenteDto> CreerAsync(CreerVenteDto request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/ventes", request, JsonOptions.Default, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<VenteDto>(JsonOptions.Default, ct))!;
    }

    public async Task<VenteDto> ModifierAsync(int id, UpdateVenteDto request, CancellationToken ct = default)
    {
        var response = await _http.PutAsJsonAsync($"api/ventes/{id}", request, JsonOptions.Default, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<VenteDto>(JsonOptions.Default, ct))!;
    }

    public async Task<VenteDto> ValiderAsync(int id, CancellationToken ct = default)
    {
        var response = await _http.PostAsync($"api/ventes/{id}/valider", null, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<VenteDto>(JsonOptions.Default, ct))!;
    }

    public async Task<VenteDto> AnnulerAsync(int id, AnnulerVenteDto request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync($"api/ventes/{id}/annuler", request, JsonOptions.Default, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<VenteDto>(JsonOptions.Default, ct))!;
    }

    public async Task SupprimerAsync(int id, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync($"api/ventes/{id}", ct);
        await EnsureSuccessOrThrowAsync(response, ct);
    }

    private static async Task EnsureSuccessOrThrowAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode) return;
        var body = await response.Content.ReadAsStringAsync(ct);
        throw new HttpRequestException($"Erreur API ({(int)response.StatusCode}) : {body}");
    }
}
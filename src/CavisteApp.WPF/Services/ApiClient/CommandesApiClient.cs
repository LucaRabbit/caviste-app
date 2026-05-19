using System.Net.Http;
using System.Net.Http.Json;
using CavisteApp.DTOs.Commandes;
using CavisteApp.DTOs.Enums;
using CavisteApp.DTOs.Ventes;

namespace CavisteApp.WPF.Services.ApiClient;

public class CommandesApiClient : ICommandesApiClient
{
    private readonly HttpClient _http;
    public CommandesApiClient(HttpClient http) => _http = http;

    public async Task<List<CommandeResumeDto>> GetTousAsync(StatutCommande? statut = null, CancellationToken ct = default)
    {
        var url = statut.HasValue ? $"api/commandes?statut={statut.Value}" : "api/commandes";
        var commandes = await _http.GetFromJsonAsync<List<CommandeResumeDto>>(url, JsonOptions.Default, ct);
        return commandes ?? new List<CommandeResumeDto>();
    }

    public async Task<CommandeDto?> GetParIdAsync(int id, CancellationToken ct = default)
    {
        var response = await _http.GetAsync($"api/commandes/{id}", ct);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        await EnsureSuccessOrThrowAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<CommandeDto>(JsonOptions.Default, ct);
    }

    public async Task<CommandeDto> CreerAsync(CreerCommandeDto request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/commandes", request, JsonOptions.Default, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<CommandeDto>(JsonOptions.Default, ct))!;
    }

    public async Task<CommandeDto> ModifierAsync(int id, UpdateCommandeDto request, CancellationToken ct = default)
    {
        var response = await _http.PutAsJsonAsync($"api/commandes/{id}", request, JsonOptions.Default, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<CommandeDto>(JsonOptions.Default, ct))!;
    }

    public Task<CommandeDto> ValiderAsync(int id, CancellationToken ct = default) =>
        TransitionAsync(id, "valider", ct);

    public async Task<CommandeDto> ReceptionnerAsync(int id, ReceptionnerCommandeDto request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync($"api/commandes/{id}/receptionner", request, JsonOptions.Default, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<CommandeDto>(JsonOptions.Default, ct))!;
    }

    public async Task SupprimerAsync(int id, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync($"api/commandes/{id}", ct);
        await EnsureSuccessOrThrowAsync(response, ct);
    }

    private async Task<CommandeDto> TransitionAsync(int id, string action, CancellationToken ct)
    {
        var response = await _http.PostAsJsonAsync($"api/commandes/{id}/{action}", new { }, JsonOptions.Default, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<CommandeDto>(JsonOptions.Default, ct))!;
    }

    private static async Task EnsureSuccessOrThrowAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode) return;
        var body = await response.Content.ReadAsStringAsync(ct);
        throw new HttpRequestException($"Erreur API ({(int)response.StatusCode}) : {body}");
    }

    public async Task<CommandeDto> AnnulerAsync(int id, AnnulerCommandeDto request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync($"api/commandes/{id}/annuler", request, JsonOptions.Default, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<CommandeDto>(JsonOptions.Default, ct))!;
    }
}
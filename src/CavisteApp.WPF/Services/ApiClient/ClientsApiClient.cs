using System.Net.Http;
using System.Net.Http.Json;
using CavisteApp.DTOs.Clients;

namespace CavisteApp.WPF.Services.ApiClient;
// Implémentation du client API pour les opérations sur les vins, utilisant HttpClient pour communiquer avec l'API REST
public class ClientsApiClient : IClientsApiClient
{
    private readonly HttpClient _http;

    public ClientsApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<ClientDto>> GetTousAsync(CancellationToken ct = default)
    {
        var vins = await _http.GetFromJsonAsync<List<ClientDto>>("api/clients", JsonOptions.Default, ct);
        return vins ?? new List<ClientDto>();
    }

    public async Task<ClientDto?> GetParIdAsync(int id, CancellationToken ct = default)
    {
        var response = await _http.GetAsync($"api/clients/{id}", ct);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ClientDto>(JsonOptions.Default, ct);
    }

    public async Task<ClientDto> CreerAsync(CreerClientDto request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/clients", request, JsonOptions.Default, ct);
        //response.EnsureSuccessStatusCode();

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException(
                $"Erreur API ({(int)response.StatusCode}) : {body}");
        }

        return (await response.Content.ReadFromJsonAsync<ClientDto>(JsonOptions.Default, ct))!;
    }

    public async Task ModifierAsync(int id, UpdateClientDto request, CancellationToken ct = default)
    {
        var response = await _http.PutAsJsonAsync($"api/clients/{id}", request, JsonOptions.Default, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task SupprimerAsync(int id, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync($"api/clients/{id}", ct);
        await EnsureSuccessOrThrowAsync(response, ct);
    }

    private static async Task EnsureSuccessOrThrowAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode) return;
        var body = await response.Content.ReadAsStringAsync(ct);
        throw new HttpRequestException($"Erreur API ({(int)response.StatusCode}) : {body}");
    }
}
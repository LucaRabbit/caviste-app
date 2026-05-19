using System.Net.Http;
using System.Net.Http.Json;
using CavisteApp.DTOs.Import;

namespace CavisteApp.WPF.Services.ApiClient;

public class ImportApiClient : IImportApiClient
{
    private readonly HttpClient _http;
    public ImportApiClient(HttpClient http) => _http = http;

    public async Task<List<WineSearchResultDto>> RechercherAsync(string q, int limite, CancellationToken ct = default)
    {
        var url = $"api/import/wineapi/recherche?q={Uri.EscapeDataString(q)}&limite={limite}";
        var response = await _http.GetAsync(url, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<List<WineSearchResultDto>>(JsonOptions.Default, ct)
               ?? new List<WineSearchResultDto>();
    }

    public async Task<ImportResultDto> ImporterAsync(string q, int limite, CancellationToken ct = default)
    {
        var url = $"api/import/wineapi?q={Uri.EscapeDataString(q)}&limite={limite}";
        var response = await _http.PostAsync(url, null, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<ImportResultDto>(JsonOptions.Default, ct))!;
    }

    private static async Task EnsureSuccessOrThrowAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode) return;
        var body = await response.Content.ReadAsStringAsync(ct);
        throw new HttpRequestException($"Erreur API ({(int)response.StatusCode}) : {body}");
    }
}
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace CavisteApp.Api.ExternalApi;

public class WineApiClient : IWineApiClient
{
    // Options de désérialisation JSON : "Web" gère le camelCase automatiquement
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;

    // HttpClient est injecté par ASP.NET grâce à AddHttpClient<IWineApiClient, WineApiClient> dans Program.cs
    public WineApiClient(HttpClient http) => _http = http;


    public async Task<WineSearchResponse> SearchAsync(string query, int limit = 20, CancellationToken ct = default)
    {
        // BaseAddress = "https://api.wineapi.io/" → URL finale = "https://api.wineapi.io/wines/search?q=..."
        var url = $"wines/search?q={Uri.EscapeDataString(query)}&limit={limit}";

        // GetFromJsonAsync : appel GET + désérialisation JSON en une ligne
        var res = await _http.GetFromJsonAsync<WineSearchResponse>(url, JsonOpts, ct);

        // Si l'API renvoie un corps vide ou null, on retourne un objet vide plutôt que null
        return res ?? new WineSearchResponse();
    }

    public async Task<WineDetailsDto?> GetDetailsAsync(string wineId, CancellationToken ct = default)
    {
        // Ici on fait GetAsync (et pas GetFromJsonAsync) parce qu'on veut traiter le 404 spécifiquement :
        // un vin inconnu n'est pas une erreur, c'est juste "pas trouvé" → on renvoie null.
        var res = await _http.GetAsync($"wines/{wineId}", ct);

        if (res.StatusCode == HttpStatusCode.NotFound)
            return null;

        // Pour toute autre erreur (401 clé invalide, 429 rate limit, 500…), on lève une exception
        // qui remontera jusqu'au controller (ou jusqu'à un middleware d'erreur).
        res.EnsureSuccessStatusCode();

        return await res.Content.ReadFromJsonAsync<WineDetailsDto>(JsonOpts, ct);
    }
}
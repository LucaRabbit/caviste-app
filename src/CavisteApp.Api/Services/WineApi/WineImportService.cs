using CavisteApp.Api.Data;

namespace CavisteApp.Api.Services.WineApi;

public class WineImportService
{
    private readonly IWineApiClient _api;
    private readonly CavisteDbContext _db;
    private readonly ILogger<WineImportService> _log;

    public WineImportService(IWineApiClient api, CavisteDbContext db, ILogger<WineImportService> log)
    { _api = api; _db = db; _log = log; }

    public async Task<int> ImporterAsync(string query, int limite, CancellationToken ct)
    {
        var recherche = await _api.SearchAsync(query, limite, ct);
        int ajoutes = 0;

        foreach (var hit in recherche.Results)
        {
            // Idempotence : ajout SourceExterneId sur Vin, vérifier ici
            // if (_db.Vins.Any(v => v.SourceExterneId == hit.Id)) continue;

            var details = await _api.GetDetailsAsync(hit.Id, ct);
            if (details is null) { _log.LogWarning("Vin {Id} introuvable", hit.Id); continue; }

            _db.Vins.Add(VinMapper.ToEntity(details));
            ajoutes++;
        }

        await _db.SaveChangesAsync(ct);
        return ajoutes;
    }
}
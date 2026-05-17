using CavisteApp.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace CavisteApp.Api.Services.WineApi;

public class WineImportService
{
    private readonly IWineApiClient _api;
    private readonly CavisteDbContext _db;
    private readonly ILogger<WineImportService> _log;

    public WineImportService(IWineApiClient api, CavisteDbContext db, ILogger<WineImportService> log)
    { _api = api; _db = db; _log = log; }

    public async Task<ImportResultat> ImporterAsync(string query, int limite, CancellationToken ct)
    {
        var recherche = await _api.SearchAsync(query, limite, ct);

        // Récupérer en une requête tous les SourceExterneId déjà connus
        var idsExternesRecherches = recherche.Results.Select(r => r.Id).ToList();
        var idsDejaPresents = await _db.Vins
            .Where(v => v.SourceExterneId != null && idsExternesRecherches.Contains(v.SourceExterneId))
            .Select(v => v.SourceExterneId!)
            .ToHashSetAsync<string>(ct);

        int ajoutes = 0;
        int ignores = 0;

        foreach (var hit in recherche.Results)
        {
            if (idsDejaPresents.Contains(hit.Id))
            {
                ignores++;
                _log.LogInformation("Vin {Id} déjà importé, ignoré", hit.Id);
                continue;
            }

            var details = await _api.GetDetailsAsync(hit.Id, ct);
            if (details is null) continue;

            _db.Vins.Add(VinMapper.ToEntity(details));
            ajoutes++;
        }

        await _db.SaveChangesAsync(ct);
        return new ImportResultat(ajoutes, ignores);
    }
}

public record ImportResultat(int Ajoutes, int Ignores);
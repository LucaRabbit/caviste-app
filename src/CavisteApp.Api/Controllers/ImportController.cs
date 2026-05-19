using CavisteApp.Api.Services.WineApi;
using Microsoft.AspNetCore.Mvc;

[ApiController, Route("api/[controller]")]
public class ImportController : ControllerBase
{
    private readonly WineImportService _import;
    private readonly IWineApiClient _wineApi;
    public ImportController(WineImportService import, IWineApiClient wineApi)
    {
        _import = import;
        _wineApi = wineApi;
    }

    // POST: api/import/wineapi?q=merlot&limite=10
    [HttpPost("wineapi")]
    public async Task<IActionResult> Importer(
        [FromQuery] string q,
        [FromQuery] int limite = 20,
        CancellationToken ct = default)
    {
        var resultat = await _import.ImporterAsync(q, limite, ct);
        return Ok(new CavisteApp.DTOs.Import.ImportResultDto
        {
            Ajoutes = resultat.Ajoutes,
            Ignores = resultat.Ignores,
            Message = $"{resultat.Ajoutes} vin(s) ajouté(s), {resultat.Ignores} doublon(s) ignoré(s)"
        });
    }

    // ImportController.cs
    [HttpGet("wineapi/recherche")]
    public async Task<IActionResult> Rechercher(
        [FromQuery] string q,
        [FromQuery] int limite = 20,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("Le mot-clé de recherche est requis.");

        var resultat = await _wineApi.SearchAsync(q, limite, ct);

        // Mapping API externe → DTO partagé
        var dtos = resultat.Results.Select(r => new CavisteApp.DTOs.Import.WineSearchResultDto
        {
            Id = r.Id,
            Name = r.Name,
            Vintage = r.Vintage,
            Type = r.Type,
            Winery = r.Winery,
            Region = r.Region,
            Country = r.Country,
            AverageRating = r.AverageRating
        }).ToList();

        return Ok(dtos);
    }
}
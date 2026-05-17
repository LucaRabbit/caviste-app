using CavisteApp.Api.Services.WineApi;
using Microsoft.AspNetCore.Mvc;

[ApiController, Route("api/[controller]")]
public class ImportController : ControllerBase
{
    private readonly WineImportService _import;
    public ImportController(WineImportService s) => _import = s;

    // POST: api/import/wineapi?q=merlot&limite=10
    [HttpPost("wineapi")]
    public async Task<IActionResult> Importer(
        [FromQuery] string q,
        [FromQuery] int limite = 20,
        CancellationToken ct = default)
    {
        var resultat = await _import.ImporterAsync(q, limite, ct);
        return Ok(new
        {
            ajoutes = resultat.Ajoutes,
            ignores = resultat.Ignores,
            message = $"{resultat.Ajoutes} vin(s) ajouté(s), {resultat.Ignores} doublon(s) ignoré(s)"
        });
    }
}
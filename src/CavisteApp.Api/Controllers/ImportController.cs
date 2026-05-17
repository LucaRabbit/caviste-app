using CavisteApp.Api.Services.WineApi;
using Microsoft.AspNetCore.Mvc;

[ApiController, Route("api/[controller]")]
public class ImportController : ControllerBase
{
    private readonly WineImportService _import;
    public ImportController(WineImportService s) => _import = s;

    [HttpPost("wineapi")]
    public async Task<IActionResult> Importer([FromQuery] string q, [FromQuery] int limite = 20, CancellationToken ct = default)
        => Ok(new { ajoutes = await _import.ImporterAsync(q, limite, ct) });
}
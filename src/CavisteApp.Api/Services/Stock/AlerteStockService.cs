// CavisteApp.Api/Services/Stock/AlerteStockService.cs
using CavisteApp.Api.Entities;
using CavisteApp.Api.Services.Email;

namespace CavisteApp.Api.Services.Stock;

public class AlerteStockService
{
    private readonly IEmailService _email;
    private readonly ILogger<AlerteStockService> _log;

    public AlerteStockService(IEmailService email, ILogger<AlerteStockService> log)
    {
        _email = email;
        _log = log;
    }

    // À appeler APRÈS modification du stock d'un vin
    // Envoie un mail uniquement si le vin vient de basculer en stock bas
    public async Task VerifierEtAlerterAsync(Vin vin, int stockAvant, CancellationToken ct = default)
    {
        bool etaitStockBas = stockAvant <= vin.SeuilStockBas;
        bool estDesormaisBas = vin.EstStockBas();

        // Transition false → true uniquement
        if (!etaitStockBas && estDesormaisBas)
        {
            var sujet = $"[Caviste] Stock bas : {vin.Nom}";
            var corps = ConstruireCorps(vin);
            await _email.EnvoyerAsync(sujet, corps, ct);
        }
    }

    private static string ConstruireCorps(Vin vin) => $"""
        <h2>Alerte stock bas</h2>
        <p>Le vin <strong>{vin.Nom}</strong> est passé sous le seuil de stock bas.</p>
        <ul>
            <li>Stock restant : <strong>{vin.Stock}</strong></li>
            <li>Seuil configuré : {vin.SeuilStockBas}</li>
            <li>Type : {vin.Type}</li>
            <li>Prix unitaire : {vin.Prix:C}</li>
        </ul>
        <p>Pensez à réapprovisionner ce vin.</p>
        """;
}
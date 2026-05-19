using CavisteApp.DTOs.Vins;

namespace CavisteApp.WPF.Services.ApiClient;

// Interface définissant les opérations disponibles pour interagir avec l'API REST des vins, utilisée par le ViewModel pour effectuer les appels API de manière abstraite
public interface IVinsApiClient
{
    Task<List<VinDto>> GetTousAsync(CancellationToken ct = default);
    Task<VinDto?> GetParIdAsync(int id, CancellationToken ct = default);
    Task<VinDto> CreerAsync(CreerVinDto request, CancellationToken ct = default);
    Task ModifierAsync(int id, UpdateVinDto request, CancellationToken ct = default);
    Task SupprimerAsync(int id, CancellationToken ct = default);
    Task AjusterStockAsync(int id, InventaireDto request, CancellationToken ct = default);
    Task RetirerStockAsync(int id, RetraitStockDto request, CancellationToken ct = default);
}
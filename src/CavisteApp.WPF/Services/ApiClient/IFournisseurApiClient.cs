using CavisteApp.DTOs.Fournisseurs;
using CavisteApp.DTOs.Vins;

namespace CavisteApp.WPF.Services.ApiClient;

// Interface définissant les opérations disponibles pour interagir avec l'API REST des fournisseurs, utilisée par le ViewModel pour effectuer les appels API de manière abstraite
public interface IFournisseursApiClient
{
    Task<List<FournisseurDto>> GetTousAsync(CancellationToken ct = default);
    Task<FournisseurDto?> GetParIdAsync(int id, CancellationToken ct = default);
    Task<FournisseurDto> CreerAsync(CreerFournisseurDto request, CancellationToken ct = default);
    Task ModifierAsync(int id, UpdateFournisseurDto request, CancellationToken ct = default);
    Task SupprimerAsync(int id, CancellationToken ct = default);
    Task AjusterStockAsync(int id, InventaireDto request, CancellationToken ct = default);
}
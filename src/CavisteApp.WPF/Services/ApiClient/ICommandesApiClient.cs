using CavisteApp.DTOs.Commandes;
using CavisteApp.DTOs.Enums;
using CavisteApp.DTOs.Ventes;

namespace CavisteApp.WPF.Services.ApiClient;

public interface ICommandesApiClient
{
    Task<List<CommandeResumeDto>> GetTousAsync(StatutCommande? statut = null, CancellationToken ct = default);
    Task<CommandeDto?> GetParIdAsync(int id, CancellationToken ct = default);
    Task<CommandeDto> CreerAsync(CreerCommandeDto request, CancellationToken ct = default);
    Task<CommandeDto> ModifierAsync(int id, UpdateCommandeDto request, CancellationToken ct = default);
    Task<CommandeDto> AnnulerAsync(int id, AnnulerCommandeDto request, CancellationToken ct = default);
    Task<CommandeDto> ValiderAsync(int id, CancellationToken ct = default);
    Task<CommandeDto> ReceptionnerAsync(int id, ReceptionnerCommandeDto request, CancellationToken ct = default);
    Task SupprimerAsync(int id, CancellationToken ct = default);
}
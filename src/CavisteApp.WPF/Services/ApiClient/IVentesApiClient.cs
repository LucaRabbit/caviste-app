using CavisteApp.Api.Dtos.Ventes;
using CavisteApp.DTOs.Enums;
using CavisteApp.DTOs.Ventes;

namespace CavisteApp.WPF.Services.ApiClient;

public interface IVentesApiClient
{
    Task<List<VenteResumeDto>> GetTousAsync(StatutVente? statut = null, int? clientId = null, CancellationToken ct = default);
    Task<VenteDto?> GetParIdAsync(int id, CancellationToken ct = default);
    Task<VenteDto> CreerAsync(CreerVenteDto request, CancellationToken ct = default);
    Task<VenteDto> ModifierAsync(int id, UpdateVenteDto request, CancellationToken ct = default);
    Task<VenteDto> ValiderAsync(int id, CancellationToken ct = default);
    Task<VenteDto> AnnulerAsync(int id, AnnulerVenteDto request, CancellationToken ct = default);
    Task SupprimerAsync(int id, CancellationToken ct = default);
}
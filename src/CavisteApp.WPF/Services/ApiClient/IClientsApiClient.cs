using CavisteApp.DTOs.Clients;

namespace CavisteApp.WPF.Services.ApiClient;

// Interface définissant les opérations disponibles pour interagir avec l'API REST des vins, utilisée par le ViewModel pour effectuer les appels API de manière abstraite
public interface IClientsApiClient
{
    Task<List<ClientDto>> GetTousAsync(CancellationToken ct = default);
    Task<ClientDto?> GetParIdAsync(int id, CancellationToken ct = default);
    Task<ClientDto> CreerAsync(CreerClientDto request, CancellationToken ct = default);
    Task ModifierAsync(int id, UpdateClientDto request, CancellationToken ct = default);
    Task SupprimerAsync(int id, CancellationToken ct = default);
}
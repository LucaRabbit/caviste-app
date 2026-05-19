using CavisteApp.DTOs.Import;

namespace CavisteApp.WPF.Services.ApiClient;

public interface IImportApiClient
{
    Task<List<WineSearchResultDto>> RechercherAsync(string q, int limite, CancellationToken ct = default);
    Task<ImportResultDto> ImporterAsync(string q, int limite, CancellationToken ct = default);
}
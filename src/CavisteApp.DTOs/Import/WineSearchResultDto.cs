namespace CavisteApp.DTOs.Import;

public class WineSearchResultDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? Vintage { get; set; }
    public string? Type { get; set; }
    public string? Winery { get; set; }
    public string? Region { get; set; }
    public string? Country { get; set; }
    public double? AverageRating { get; set; }
}

public class ImportResultDto
{
    public int Ajoutes { get; set; }
    public int Ignores { get; set; }
    public string Message { get; set; } = string.Empty;
}
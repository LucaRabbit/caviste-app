namespace CavisteApp.Api.ExternalApi;

public class WineSearchResponse
{
    public List<WineSearchResultDto> Results { get; set; } = new();
    public int Total { get; set; }
    public int Limit { get; set; }
    public int Offset { get; set; }
}

public class WineSearchResultDto
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public int? Vintage { get; set; }
    public string? Type { get; set; }
    public string? Winery { get; set; }
    public string? Region { get; set; }
    public string? Country { get; set; }
    public double? AverageRating { get; set; }
    public int? RatingsCount { get; set; }
    public double Confidence { get; set; }
}

public class WineDetailsDto
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public int? Vintage { get; set; }
    public string? Type { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public WineryDto? Winery { get; set; }
    public RegionDto? Region { get; set; }
    public List<GrapeDto> Grapes { get; set; } = new();
    public PriceRangeDto? PriceRange { get; set; }
    public List<PriceDto> Prices { get; set; } = new();
}

public class WineryDto { public string Id { get; set; } = default!; public string Name { get; set; } = default!; }
public class RegionDto { public string Id { get; set; } = default!; public string Name { get; set; } = default!; public string? Country { get; set; } }
public class GrapeDto { public string Id { get; set; } = default!; public string Name { get; set; } = default!; public string? Color { get; set; } }
public class PriceRangeDto { public decimal Min { get; set; } public decimal Max { get; set; } public string Currency { get; set; } = "EUR"; }
public class PriceDto { public string? MerchantName { get; set; } public decimal Price { get; set; } public string Currency { get; set; } = "EUR"; }
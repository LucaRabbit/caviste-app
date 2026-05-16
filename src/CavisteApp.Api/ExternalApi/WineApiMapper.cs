using CavisteApp.Api.Entities;
using CavisteApp.Api.Enums;
using CavisteApp.Api.ExternalApi;

public static class VinMapper
{
    public static Vin ToEntity(WineDetailsDto src)
    {
        return new Vin
        {
            // Depuis l'API externe
            Nom = ComposerNom(src),
            Type = MapType(src.Type),
            Prix = ChoisirPrix(src),

            // Champs métier — valeurs par défaut, à ajuster côté WPF
            Stock = 0,
            SeuilStockBas = 5,
            DateCreation = DateTime.UtcNow
        };
    }

    private static string ComposerNom(WineDetailsDto src)
    {
        var nom = src.Name;
        if (src.Vintage.HasValue) nom += $" {src.Vintage.Value}";
        return nom;
    }

    private static TypeVin MapType(string? type) => type?.Trim().ToLowerInvariant() switch
    {
        "red" => TypeVin.Rouge,
        "white" => TypeVin.Blanc,
        "rose" => TypeVin.Rose,
        "sparkling" => TypeVin.Petillant,
        "dessert" => TypeVin.Autre, // ajouter Doux aux enums si besoin
        _ => TypeVin.Autre
    };

    private static decimal ChoisirPrix(WineDetailsDto src)
    {
        if (src.PriceRange is { Min: > 0 }) return src.PriceRange.Min;
        if (src.Prices.Count > 0) return src.Prices.Average(p => p.Price);
        return 0m;                          // à compléter manuellement ensuite
    }
}
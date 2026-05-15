using System.Text.Json.Serialization;

namespace CavisteApp.Api.Entities;

public class LigneVente
{
    public int Id { get; set; }

    // Snapshot du nom du vin au moment de la vente (pour éviter les problèmes si changement dans la base de données)
    public string VinNom { get; set; } = string.Empty;
    public int Quantite { get; set; }
    public decimal PrixUnitaire { get; set; }

    public int VenteId { get; set; }
    [JsonIgnore] // Pour éviter les références circulaires lors de la sérialisation
    public Vente Vente { get; set; } = null!;

    // Référence au vin vendu (pour les détails et la validation du stock)
    public int VinId { get; set; }
    [JsonIgnore] // Pour éviter les références circulaires lors de la sérialisation
    public Vin Vin { get; set; } = null!;

    // Propriété calculée pour le sous-total de la ligne (quantité * prix unitaire)
    public decimal SousTotal => Quantite * PrixUnitaire;

}

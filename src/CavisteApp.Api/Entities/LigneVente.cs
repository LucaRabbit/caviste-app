namespace CavisteApp.Api.Entities;

public class LigneVente
{
    public int Id { get; set; }

    // Snapshot du nom du vin au moment de la vente (pour éviter les problèmes si changement dans la base de données)
    public string VinNom { get; set; } = string.Empty;
    public int Quantite { get; set; }
    public decimal PrixUnitaire { get; set; }

    public int VenteId { get; set; }
    public Vente Vente { get; set; } = null!;

    public int VinId { get; set; }
    public Vin Vin { get; set; } = null!;

    public decimal SousTotal => Quantite * PrixUnitaire;

}

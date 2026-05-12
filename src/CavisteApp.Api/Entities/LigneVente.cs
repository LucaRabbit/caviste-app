namespace CavisteApp.Api.Entities;

public class LigneVente
{
    public int Id { get; set; }
    public int Quantite { get; set; }
    public decimal PrixUnitaire { get; set; }

    public int VenteId { get; set; }
    public Vente Vente { get; set; } = null!;

    public int VinId { get; set; }
    public Vin Vin { get; set; } = null!;

    public decimal SousTotal => Quantite * PrixUnitaire;

}

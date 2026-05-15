// LigneVenteDto
namespace CavisteApp.DTOs.Ventes;

public class LigneVenteDto
{
    public int Id { get; set; }
    public int VinId { get; set; }

    // Snapshot du nom du vin au moment de la vente (pour éviter les problèmes si changement dans la base de données)
    public string VinNom { get; set; } = string.Empty;
    public int Quantite { get; set; }
    public decimal PrixUnitaire { get; set; }
    public decimal SousTotal => PrixUnitaire * Quantite;
}

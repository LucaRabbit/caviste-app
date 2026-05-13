// LigneVenteDto
namespace CavisteApp.DTOs.Ventes;

public class LigneVenteDto
{
    public int Id { get; set; }
    public int VinId { get; set; }
    public string VinNom { get; set; } = string.Empty;
    public int Quantite { get; set; }
    public decimal PrixUnitaire { get; set; }
    public decimal SousTotal => PrixUnitaire * Quantite;
}

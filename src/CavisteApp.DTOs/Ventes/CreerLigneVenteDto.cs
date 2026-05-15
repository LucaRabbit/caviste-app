// CreerLigneVenteDto : DTO pour la création d'une ligne de vente
namespace CavisteApp.DTOs.Ventes;

public class CreerLigneVenteDto
{
    public int VinId { get; set; }
    public int Quantite { get; set; }
    public decimal PrixUnitaire { get; set; }
}

using System.ComponentModel.DataAnnotations;

// CreerLigneVenteDto : DTO pour la création d'une ligne de vente
namespace CavisteApp.DTOs.Ventes;

public class CreerLigneVenteDto
{
    [Required]
    public int VinId { get; set; }
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "La quantité doit être au moins de 1.")]
    public int Quantite { get; set; }
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Le prix unitaire doit être supérieur à zéro.")]
    public decimal PrixUnitaire { get; set; }
}

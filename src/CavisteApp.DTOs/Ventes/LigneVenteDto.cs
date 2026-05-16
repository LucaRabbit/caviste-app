using System.ComponentModel.DataAnnotations;

// LigneVenteDto
namespace CavisteApp.DTOs.Ventes;

public class LigneVenteDto
{
    [Required]
    public int Id { get; set; }
    [Required]
    public int VinId { get; set; }

    // Snapshot du nom du vin au moment de la vente (pour éviter les problèmes si changement dans la base de données)
    [Required]
    [StringLength(100, ErrorMessage = "Le nom du vin ne peut pas dépasser 100 caractères.")]
    public string VinNom { get; set; } = string.Empty;
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "La quantité doit être au moins de 1.")]
    public int Quantite { get; set; }
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Le prix unitaire doit être supérieur à zéro.")]
    public decimal PrixUnitaire { get; set; }
    [Required]
    public decimal SousTotal => PrixUnitaire * Quantite;
}

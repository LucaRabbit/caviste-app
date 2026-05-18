using System.ComponentModel.DataAnnotations;
using CavisteApp.DTOs.Enums;

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
    [StringLength(100,ErrorMessage = "Le nom du vin est limité à 100 caractères")]
    public string VinNom { get; set; } = string.Empty;
    [Required]
    public decimal PrixUnitaire { get; set; }
    [Required]
    public TypeVin Type { get; set; }
    [Required]
    [Range(1,int.MaxValue)]
    public int Quantite { get; set; }

    public decimal SousTotal => PrixUnitaire * Quantite;
}

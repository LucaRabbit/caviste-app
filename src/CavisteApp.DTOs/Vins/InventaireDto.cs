using System.ComponentModel.DataAnnotations;

namespace CavisteApp.DTOs.Vins;

// DTO pour corriger le stock à une valeur réelle constatée lors d'un inventaire
public class InventaireDto
{
    // Stock réel constaté en rayon/cave
    [Required]
    [Range(0, 100000, ErrorMessage = "Le stock doit être positif ou nul.")]
    public int StockReel { get; set; }

    // Commentaire optionnel justifiant l'écart
    [StringLength(500)]
    public string? Commentaire { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace CavisteApp.DTOs.Vins;

// DTO pour réceptionner du stock (livraison fournisseur, retour client, etc.)
public class ReceptionStockDto
{
    // Quantité reçue (toujours positive)
    [Required]
    [Range(1, 10000, ErrorMessage = "La quantité doit être comprise entre 1 et 10000.")]
    public int Quantite { get; set; }

    // Référence interne : n° bon de livraison, commande fournisseur, etc
    [StringLength(100)]
    public string? Reference { get; set; }
}
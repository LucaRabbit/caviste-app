using System.ComponentModel.DataAnnotations;

namespace CavisteApp.Api.Dtos.Vins;

// DTO pour retirer du stock pour un motif autre qu'une vente (casse, perte, etc.)
public class RetraitStockDto
{
    // Quantité à retirer (toujours positive, le retrait est implicite)
    [Required]
    [Range(1, 10000, ErrorMessage = "La quantité doit être comprise entre 1 et 10000.")]
    public int Quantite { get; set; }

    // Motif du retrait
    [Required]
    public int Motif { get; set; }

    // Commentaire optionnel (détails sur la casse, etc.)
    [StringLength(500)]
    public string? Commentaire { get; set; }
}
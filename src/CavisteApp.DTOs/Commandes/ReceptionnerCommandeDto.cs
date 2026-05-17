using System.ComponentModel.DataAnnotations;

namespace CavisteApp.DTOs.Commandes;

public class ReceptionnerCommandeDto
{
    // Quantités réellement reçues pour chaque ligne
    // Si une ligne n'est pas dans cette liste, la quantité commandée est prise par défaut
    public List<ReceptionLigneDto> Lignes { get; set; } = new();

    // Commentaire optionnel (bon de livraison, anomalies, etc.)
    [StringLength(500)]
    public string? Commentaire { get; set; }
}

public class ReceptionLigneDto
{
    [Required]
    public int Id { get; set; }

    [Required]
    [Range(0, 10000, ErrorMessage = "La quantité reçue doit être positive ou nulle.")]
    public int QuantiteRecue { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace CavisteApp.DTOs.Commandes;

public class AnnulerCommandeDto
{
    [Required]
    [StringLength(500, MinimumLength = 3)]
    public string Motif { get; set; } = string.Empty;
}
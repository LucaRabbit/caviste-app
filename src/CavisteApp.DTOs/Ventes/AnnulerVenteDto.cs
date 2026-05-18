using System.ComponentModel.DataAnnotations;

namespace CavisteApp.DTOs.Ventes;

public class AnnulerVenteDto
{
    [Required]
    [StringLength(500, MinimumLength = 3)]
    public string Motif { get; set; } = string.Empty;
}
using System.ComponentModel.DataAnnotations;

namespace CavisteApp.Api.Dtos.Ventes;

public class AnnulerVenteDto
{
    [Required]
    [StringLength(500, MinimumLength = 3)]
    public string Motif { get; set; } = string.Empty;
}
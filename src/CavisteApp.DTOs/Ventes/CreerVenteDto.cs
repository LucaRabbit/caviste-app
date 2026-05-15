using System.ComponentModel.DataAnnotations;

// CréerVenteDto : DTO pour la création d'une vente
namespace CavisteApp.DTOs.Ventes;
public class CreerVenteDto
{
    [Required]
    public int ClientId { get; set; }
    [Required]
    public List<CreerLigneVenteDto> Lignes { get; set; } = new();
}

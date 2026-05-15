// CréerVenteDto : DTO pour la création d'une vente
namespace CavisteApp.DTOs.Ventes;
public class CreerVenteDto
{
    public int ClientId { get; set; }
    public List<CreerLigneVenteDto> Lignes { get; set; } = new();
}

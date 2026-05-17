using System.ComponentModel.DataAnnotations;

namespace CavisteApp.Api.Dtos.Ventes;

public class UpdateVenteDto
{
    [Required]
    public int ClientId { get; set; }

    [Required]
    [MinLength(1)]
    public List<UpdateLigneVenteDto> Lignes { get; set; } = new();
}

public class UpdateLigneVenteDto
{
    //Id de la ligne existante (null pour une nouvelle ligne).
    public int? Id { get; set; }

    [Required]
    public int VinId { get; set; }

    [Required]
    [Range(1, 10000)]
    public int Quantite { get; set; }

    [Range(0.01, 100000)]
    public decimal PrixUnitaire { get; set; }
}
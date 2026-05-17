using System.ComponentModel.DataAnnotations;

namespace CavisteApp.Api.Dtos.Commandes;

public class UpdateCommandeDto
{
    [Required]
    public int FournisseurId { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "Une commande doit contenir au moins une ligne.")]
    public List<UpdateLigneCommandeDto> Lignes { get; set; } = new();
}

public class UpdateLigneCommandeDto
{
    // Id de la ligne existante (null pour une nouvelle ligne).
    public int? Id { get; set; }

    [Required]
    public int VinId { get; set; }

    [Required]
    [Range(1, 10000)]
    public int Quantite { get; set; }

    [Range(0.01, 100000)]
    public decimal PrixUnitaire { get; set; }
}
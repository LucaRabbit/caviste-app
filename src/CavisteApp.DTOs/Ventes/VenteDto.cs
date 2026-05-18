// VenteDto
using System.ComponentModel.DataAnnotations;
using CavisteApp.DTOs.Enums;

namespace CavisteApp.DTOs.Ventes;

public class VenteDto
{
    [Required]
    public int Id { get; set; }
    public DateTime Date{ get; set; }
    [Required]
    public decimal MontantTotal { get; set; }
    [Required]
    public StatutVente Statut { get; set; }
    public DateTime? DateValidation { get; set; }
    [Required]
    public int ClientId { get; set; }
    [Required]
    public string ClientNom { get; set; } = string.Empty;
    [Required]
    public int UtilisateurId { get; set; }
    [Required]
    public string UtilisateurNom { get; set; } = string.Empty;
    [Required]
    public List<LigneVenteDto> Lignes { get; set; } = new();

}
// VenteResumeDto : DTO pour le résumé d'une vente (sans les détails des lignes)
using System.ComponentModel.DataAnnotations;
using CavisteApp.DTOs.Enums;

namespace CavisteApp.DTOs.Ventes;

public class VenteResumeDto
{
    [Required]
    public int Id { get; set; }
    public DateTime Date { get; set; }
    [Required]
    public decimal MontantTotal { get; set; }
    public StatutVente Statut { get; set; }
    [Required]
    public string ClientNom { get; set; } = string.Empty;
    [Required]
    public int NombreLignes { get; set; }
}

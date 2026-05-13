// VenteResumeDto : DTO pour le résumé d'une vente (sans les détails des lignes)
namespace CavisteApp.DTOs.Ventes;

public class VenteResumeDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public decimal MontantTotal { get; set; }
    public string ClientNom { get; set; } = string.Empty;
    public int NombreLignes { get; set; }
}

// VenteDto
using CavisteApp.DTOs.Enums;

namespace CavisteApp.DTOs.Ventes;

public class VenteDto
{
    public int Id { get; set; }
    public DateTime Date{ get; set; }
    public decimal MontantTotal { get; set; }
    public StatutVente Statut { get; set; }
    public DateTime? DateValidation { get; set; }

    public int ClientId { get; set; }
    public string ClientNom { get; set; } = string.Empty;

    public int UtilisateurId { get; set; }
    public string UtilisateurNom { get; set; } = string.Empty;

    public List<LigneVenteDto> Lignes { get; set; } = new();

}
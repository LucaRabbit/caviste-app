using CavisteApp.DTOs.Enums;

namespace CavisteApp.Api.Entities;

public class Commande
{
    public int Id { get; set; }
    public StatutCommande Statut { get; set; } = StatutCommande.Brouillon;
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    public DateTime? DateValidation { get; set; }
    public DateTime? DateReception { get; set; }
    public DateTime? DateAnnulation { get; set; }
    public string? MotifAnnulation { get; set; }

    public int FournisseurId { get; set; }
    public Fournisseur Fournisseur { get; set; } = null!;

    public ICollection<LigneCommande> Lignes { get; set; } = new List<LigneCommande>();
}

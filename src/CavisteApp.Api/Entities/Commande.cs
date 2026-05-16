using CavisteApp.Api.Enums;

namespace CavisteApp.Api.Entities;

public class Commande
{
    public int Id { get; set; }
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    public DateTime? DateValidation { get; set; }
    public DateTime? DateReception { get; set; }
    public StatutCommande Statut { get; set; } = StatutCommande.Brouillon;

    public int FournisseurId { get; set; }
    public Fournisseur Fournisseur { get; set; } = null!;

    public ICollection<LigneCommande> Lignes { get; set; } = new List<LigneCommande>();
}

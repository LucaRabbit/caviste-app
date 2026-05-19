using CavisteApp.DTOs.Enums;

namespace CavisteApp.Api.Entities;

public class Vente
{
    public int Id { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public decimal MontantTotal { get; set; }

    public StatutVente Statut { get; set; } = StatutVente.Brouillon;
    public DateTime? DateValidation { get; set; }
    public DateTime? DateAnnulation { get; set; }
    public string? MotifAnnulation { get; set; }


    public int ClientId { get; set; }
    public Client Client { get; set; } = null!;

    public int UtilisateurId { get; set; }
    public ApplicationUser Utilisateur { get; set; } = null!;

    public ICollection<LigneVente> Lignes { get; set; } = new List<LigneVente>();

    public void CalculerMontantTotal()
    {
        MontantTotal = Lignes.Sum(l => l.PrixUnitaire * l.Quantite);
    }

}

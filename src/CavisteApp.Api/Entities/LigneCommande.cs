namespace CavisteApp.Api.Entities;

public class LigneCommande
{
    public int Id { get; set; }
    public int Quantite { get; set; }

    public int CommandeFournisseurId { get; set; }
    public Commande CommandeFournisseur { get; set; } = null!;

    public int VinId { get; set; }
    public Vin Vin { get; set; } = null!;
}

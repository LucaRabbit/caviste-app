using CavisteApp.Api.Enums;

namespace CavisteApp.Api.Entities;

public class Vin
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public TypeVin Type { get; set; }
    public decimal Prix { get; set; }
    public int Stock { get; set; }
    public int SeuilStockBas { get; set; } = 5;
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    public int? FournisseurId { get; set; }
    public Fournisseur? Fournisseur { get; set; }

    public ICollection<LigneVente> LignesVente { get; set; } = new List<LigneVente>();
    public ICollection<LigneCommande> LignesCommande { get; set; } = new List<LigneCommande>();

    public bool EstStockBas() => Stock <= SeuilStockBas;
}

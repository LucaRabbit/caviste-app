namespace CavisteApp.Api.Entities;

public class Fournisseur
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telephone { get; set; } = string.Empty;
    public int NumRue { get; set; }
    public string NomRue { get; set; } = string.Empty;
    public string CodePostal { get; set; } = string.Empty;
    public string Ville { get; set; } = string.Empty;


    public ICollection<Vin> Vins { get; set; } = new List<Vin>();
    public ICollection<CommandeFournisseur> Commandes { get; set; } = new List<CommandeFournisseur>();
}

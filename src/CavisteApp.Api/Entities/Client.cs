namespace CavisteApp.Api.Entities;

public class Client
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telephone { get; set; } = string.Empty;
    public int NumRue { get; set; }
    public string NomRue { get; set; } = string.Empty;
    public string CodePostal { get; set; } = string.Empty;
    public string Ville { get; set; } = string.Empty;
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    public ICollection<Vente> Ventes { get; set; } = new List<Vente>();

    public string NomComplet => $"{Prenom} {Nom}";

}

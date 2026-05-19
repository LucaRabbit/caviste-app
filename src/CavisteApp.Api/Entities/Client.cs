using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CavisteApp.Api.Entities;

public class Client
{
    [Required]
    public int Id { get; set; }
    [Required]
    public string Nom { get; set; } = string.Empty;
    [Required]
    public string Prenom { get; set; } = string.Empty;
    [Required]
    public string Email { get; set; } = string.Empty;
    [Required]
    [MaxLength(10)]
    [MinLength(10)]
    public string Telephone { get; set; } = string.Empty;
    [Required]
    public int NumRue { get; set; }
    [Required]
    public string NomRue { get; set; } = string.Empty;
    [Required]
    public string CodePostal { get; set; } = string.Empty;
    [Required]
    public string Ville { get; set; } = string.Empty;
    [Required]
    public DateTime DateCreation { get; set; } = DateTime.Now;

    [JsonIgnore]
    public ICollection<Vente> Ventes { get; set; } = new List<Vente>();

    [JsonIgnore]
    public string NomComplet => $"{Prenom} {Nom}";

}

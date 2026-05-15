using System.Text.Json.Serialization;
using CavisteApp.Api.Enums;
using System.ComponentModel.DataAnnotations;

namespace CavisteApp.Api.Entities;

public class Vin
{
    [Required]
    public int Id { get; set; }
    [Required]
    public string Nom { get; set; } = string.Empty;
    [Required]
    public TypeVin Type { get; set; }
    [Required]
    public decimal Prix { get; set; }
    [Required]
    public int Stock { get; set; }
    [Required]
    public int SeuilStockBas { get; set; } = 5;
    [Required]
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public ICollection<LigneVente> LignesVente { get; set; } = new List<LigneVente>();
    [JsonIgnore]
    public ICollection<LigneCommande> LignesCommande { get; set; } = new List<LigneCommande>();

    public bool EstStockBas() => Stock <= SeuilStockBas;
}

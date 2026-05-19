using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using CavisteApp.DTOs.Enums;

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
    public DateTime DateCreation { get; set; } = DateTime.Now;

    public string? SourceExterneId { get; set; } // pour idempotence lors d'import depuis API externe

    [JsonIgnore]
    public ICollection<LigneVente> LignesVente { get; set; } = new List<LigneVente>();
    [JsonIgnore]
    public ICollection<LigneCommande> LignesCommande { get; set; } = new List<LigneCommande>();

    public bool EstStockBas() => Stock <= SeuilStockBas;
}

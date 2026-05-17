using System.Text.Json.Serialization;
using CavisteApp.DTOs.Enums;

namespace CavisteApp.Api.Entities;

public class LigneCommande
{
    public int Id { get; set; }
    public int VinId { get; set; }
    [JsonIgnore]
    public Vin Vin { get; set; } = null!;

    // Snapshot des données du vin au moment de la création de la ligne de commande
    public string VinNom { get; set; } = string.Empty;
    public TypeVin VinType {  get; set; }
    public decimal PrixUnitaire { get; set; }

    public int Quantite { get; set; }
    public int? QuantiteRecue { get; set; }

    public int CommandeId { get; set; }
    [JsonIgnore]
    public Commande Commande { get; set; } = null!;

}

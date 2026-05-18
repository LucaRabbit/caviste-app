using System.ComponentModel.DataAnnotations;

namespace CavisteApp.DTOs.Commandes
{
    public class LigneCommandeDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public int VinId { get; set; }

        // Snapshot des données du vin au moment de la création de la ligne de commande
        [Required]
        public string VinNom { get; set; } = string.Empty;
        [Required]
        [Range(1, int.MaxValue)]
        public int Quantite { get; set; } // Quantité commandée
        public int? QuantiteRecue { get; set; } // Quantité réellement reçue (peut être différente de la quantité commandée)
    }
}

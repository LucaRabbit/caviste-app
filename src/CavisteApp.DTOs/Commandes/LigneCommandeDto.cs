namespace CavisteApp.DTOs.Commandes
{
    public class LigneCommandeDto
    {
        public int Id { get; set; }
        public int VinId { get; set; }

        // Snapshot des données du vin au moment de la création de la ligne de commande
        public string VinNom { get; set; } = string.Empty;

        public int Quantite { get; set; } // Quantité commandée
        public int? QuantiteRecue { get; set; } // Quantité réellement reçue (peut être différente de la quantité commandée)
    }
}

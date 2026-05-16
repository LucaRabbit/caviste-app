using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CavisteApp.DTOs.Commandes
{
    public class LigneCommandeDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public int Quantite { get; set; }
        [Required]
        public int VinId { get; set; }
        [Required]
        [StringLength(100,ErrorMessage = "Le nom du vin ne peut pas dépasser 100 caractères.")]
        public string VinNom { get; set; } = string.Empty;
    }
}

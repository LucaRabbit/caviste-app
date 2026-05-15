using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace CavisteApp.DTOs.Vins
{
    public class UpdateVinDto
    {
        [Required]
        [StringLength(100, ErrorMessage = "Le nom du vin ne peut pas dépasser 100 caractères.")]
        public string Nom { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Le prix doit être supérieur à zéro.")]
        public decimal Prix { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Le stock ne peut pas être négatif.")]
        public int Stock { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Le seuil de stock bas ne peut pas être négatif.")]
        public int SeuilStockBas { get; set; } = 5;

        [Required]
        [Range(0, 4, ErrorMessage = "Le type doit être compris entre 0 et 4")]
        public int Type { get; set; }
    }
}

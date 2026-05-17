using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using CavisteApp.DTOs.Enums;

namespace CavisteApp.DTOs.Vins
{
    public class CreerVinDto
    {
        [Required]
        [StringLength(100, ErrorMessage = "Le nom du vin ne peut pas dépasser 100 caractères.")]
        public string Nom { get; set; } = string.Empty;

        [Required]
        public TypeVin Type { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Le prix doit être supérieur à zéro.")]
        public decimal Prix { get; set; }

        [Range(0, int.MaxValue)]
        public int StockInitial { get; set; } = 0;

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Le seuil de stock bas ne peut pas être négatif.")]
        public int SeuilStockBas { get; set; } = 5;

    }
}

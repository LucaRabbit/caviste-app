using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using CavisteApp.Api.Enums;


namespace CavisteApp.DTOs.Vins
{
    internal class UpdateVinDto
    {
        [Required]
        public string Nom { get; set; } = string.Empty;

        [Required]
        public decimal Prix { get; set; }

        [Required]
        public int Stock { get; set; }

        [Required]
        public int SeuilStockBas { get; set; } = 5;

        [Required]
        public TypeVin Type { get; set; }
    }
}

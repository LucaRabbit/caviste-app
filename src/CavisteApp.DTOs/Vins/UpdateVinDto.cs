using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace CavisteApp.DTOs.Vins
{
    public class UpdateVinDto
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
        public int Type { get; set; }
    }
}

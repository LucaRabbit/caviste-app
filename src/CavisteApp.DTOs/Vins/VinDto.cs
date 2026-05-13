using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace CavisteApp.DTOs.Vins
{
    public class VinDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string Nom { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public decimal Prix { get; set; }

        [Required]
        public int Stock { get; set; }

        [Required]
        public int SeuilStockBas { get; set; }

        [Required]
        public int Type { get; set; }



    }
}

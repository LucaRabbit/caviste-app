using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using CavisteApp.DTOs.Enums;

namespace CavisteApp.DTOs.Vins
{
    public class VinDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Nom { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        [Required]
        [Range(0, int.MaxValue)]
        public decimal Prix { get; set; }
        [Required]
        [Range(0,int.MaxValue)]
        public int Stock { get; set; }
        [Required]
        [Range(0, int.MaxValue)]
        public int SeuilStockBas { get; set; }
        [Required]
        public TypeVin Type { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using CavisteApp.DTOs.Enums;

namespace CavisteApp.DTOs.Vins
{
    public class VinDto
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public decimal Prix { get; set; }
        public int Stock { get; set; }
        public int SeuilStockBas { get; set; }
        public TypeVin Type { get; set; }
    }
}

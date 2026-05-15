using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

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
        public int Type { get; set; }
    }
}

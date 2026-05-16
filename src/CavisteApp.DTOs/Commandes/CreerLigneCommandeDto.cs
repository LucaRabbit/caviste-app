using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CavisteApp.DTOs.Commandes
{
    public class CreerLigneCommandeDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La quantité doit au moins de 1")]
        public int Quantite { get; set; }
        [Required]
        public int VinId { get; set; }
    }
}

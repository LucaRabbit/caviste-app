using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CavisteApp.DTOs.Commandes
{
    public class CreerCommandeDto
    {
        [Required]
        public int FournisseurId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "La commande doit contenir au moins une ligne.")]
        public List<CreerLigneCommandeDto> Lignes { get; set; } = new();
    }
}

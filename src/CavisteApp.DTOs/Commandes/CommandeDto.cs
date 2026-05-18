using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using CavisteApp.DTOs.Enums;

namespace CavisteApp.DTOs.Commandes
{
    public class CommandeDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public StatutCommande Statut { get; set; } = 0;
        public DateTime DateCreation { get; set; }
        public DateTime? DateValidation { get; set; }
        public DateTime? DateReception { get; set; }
        [Required]
        public int FournisseurId { get; set; }
        [Required]
        public string FournisseurNom { get; set; } = null!;
        [Required]

        public List<LigneCommandeDto> Lignes { get; set; } = new();
    }
}

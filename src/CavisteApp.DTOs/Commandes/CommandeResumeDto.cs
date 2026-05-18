using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using CavisteApp.DTOs.Enums;

namespace CavisteApp.DTOs.Commandes
{
    public class CommandeResumeDto
    {
        [Required]
        public int Id { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateValidation { get; set; }
        public DateTime? DateReception { get; set; }
        [Required]
        public StatutCommande Statut { get; set; } = 0;
        [Required]
        public string FournisseurNom { get; set; } = null!;
        [Required]
        public int NombreLignes { get; set; }
    }
}

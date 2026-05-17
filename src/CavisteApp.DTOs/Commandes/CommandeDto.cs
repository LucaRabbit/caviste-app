using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using CavisteApp.DTOs.Enums;

namespace CavisteApp.DTOs.Commandes
{
    public class CommandeDto
    {
        public int Id { get; set; }
        public StatutCommande Statut { get; set; } = 0;
        public DateTime DateCreation { get; set; }
        public DateTime? DateValidation { get; set; }
        public DateTime? DateReception { get; set; }

        public int FournisseurId { get; set; }
        public string FournisseurNom { get; set; } = null!;

        public List<LigneCommandeDto> Lignes { get; set; } = new();
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace CavisteApp.DTOs.Commandes
{
    public class CommandeDto
    {
        public int Id { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateValidation { get; set; }
        public DateTime? DateReception { get; set; }
        public int Statut { get; set; } = 0;

        public int FournisseurId { get; set; }
        public string FournisseurNom { get; set; } = null!;

        public List<LigneCommandeDto> Lignes { get; set; } = new();
    }
}

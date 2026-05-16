using System;
using System.Collections.Generic;
using System.Text;

namespace CavisteApp.DTOs.Commandes
{
    public class CommandeResumeDto
    {
        public int Id { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateValidation { get; set; }
        public DateTime? DateReception { get; set; }
        public int Statut { get; set; } = 0;
        public string FournisseurNom { get; set; } = null!;

        public int NombreLignes { get; set; }
    }
}

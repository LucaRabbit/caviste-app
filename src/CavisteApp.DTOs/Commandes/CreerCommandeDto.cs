using System;
using System.Collections.Generic;
using System.Text;

namespace CavisteApp.DTOs.Commandes
{
    public class CreerCommandeDto
    {
        public int FournisseurId { get; set; }

        public List<CreerLigneCommandeDto> Lignes { get; set; } = new();
    }
}

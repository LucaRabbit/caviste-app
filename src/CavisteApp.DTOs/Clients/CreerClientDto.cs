using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CavisteApp.DTOs.Clients
{
    internal class CreerClientDto
    {
        [Required]
        public string Nom { get; set; } = string.Empty;

        [Required]
        public string Prenom { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Telephone { get; set; } = string.Empty;

        [Required]
        public int NumRue { get; set; }

        [Required]
        public string NomRue { get; set; } = string.Empty;

        [Required]
        public string CodePostal { get; set; } = string.Empty;

        [Required]
        public string Ville { get; set; } = string.Empty;
    }
}

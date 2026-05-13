using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace CavisteApp.DTOs.Clients
{
    public class ClientDto
    {
        [Required]
        public int Id { get; set; }

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

        [Required]
        public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    }
}

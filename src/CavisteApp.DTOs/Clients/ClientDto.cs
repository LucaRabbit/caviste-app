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
        [StringLength(100,ErrorMessage = "Le nom ne peut pas contenir plus de 100 caractères")]
        public string Nom { get; set; } = string.Empty;
        [Required]
        [StringLength(100, ErrorMessage = "Le prenom ne peut pas contenir plus de 100 caractères")]
        public string Prenom { get; set; } = string.Empty;
        [Required]
        [StringLength(150, ErrorMessage = "L'adresse email ne peut pas contenir plus de 150 caractères")]
        public string Email { get; set; } = string.Empty;
        [Required]
        [StringLength(10,MinimumLength = 10,ErrorMessage = "Le numéro de telephone doit contenir exactement 10 caractères")]
        public string Telephone { get; set; } = string.Empty;
        [Required]
        [StringLength (100, ErrorMessage = "Le numéro de rue ne peut pas dépasser 100 caractères")]
        public int NumRue { get; set; }
        [Required]
        [StringLength (200, ErrorMessage = "Le numéro de rue ne peut pas dépasser 200 caratères")]
        public string NomRue { get; set; } = string.Empty;
        [Required]
        [StringLength (5,MinimumLength = 5, ErrorMessage = "Le code postale doit contenir exactement 5 caractères")]
        public string CodePostal { get; set; } = string.Empty;
        [Required]
        [StringLength(100, ErrorMessage = "Le nom de la ville ne peut pas dépasser 100 caractères")]
        public string Ville { get; set; } = string.Empty;
        public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    }
}

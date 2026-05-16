using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CavisteApp.DTOs.Clients
{
    public class UpdateClientDto
    {
        [Required]
        [StringLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères.")]
        public string Nom { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "Le prénom ne peut pas dépasser 100 caractères.")]
        public string Prenom { get; set; } = string.Empty;

        [Required]
        [EmailAddress(ErrorMessage = "L'adresse e-mail n'est pas valide.")]
        [StringLength(150, ErrorMessage = "L'adresse e-mail ne peut pas dépasser 150 caractères.")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(20, ErrorMessage = "Le numéro de téléphone ne peut pas dépasser 20 caractères.")]
        public string Telephone { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Le numéro de rue doit être un entier positif.")]
        public int NumRue { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "Le nom de la rue ne peut pas dépasser 200 caractères.")]
        public string NomRue { get; set; } = string.Empty;

        [Required]
        [StringLength(5, ErrorMessage = "Le code postal ne peut pas dépasser 5 caractères.")]
        [RegularExpression(@"^\d{5}$", ErrorMessage = "Le code postal doit être composé de 5 chiffres.")]
        public string CodePostal { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "Le nom de la ville ne peut pas dépasser 100 caractères.")]
        public string Ville { get; set; } = string.Empty;
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CavisteApp.DTOs.Fournisseurs;

public class FournisseurDto
{
    [Required]
    public int Id { get; set; }
    [Required]
    [StringLength(100,ErrorMessage = "Le nom ne peut pas dépasser 100 caractères")]
    public string Nom { get; set; } = string.Empty;
    [Required]
    [StringLength(150, ErrorMessage = "L'email ne peut pas dépasser 150 caractères")]
    public string Email { get; set; } = string.Empty;
    [Required]
    [StringLength(10,MinimumLength = 10, ErrorMessage = "Le numéro de téléphone doit être composé de 10 exactement")]
    public string Telephone { get; set; } = string.Empty;
    [Required]
    [StringLength(100)]
    public int NumRue { get; set; }
    [Required]
    [StringLength(200, ErrorMessage = "Le nom de la rue ne doit pas dépasser 200 caractères")]
    public string NomRue { get; set; } = string.Empty;
    [Required]
    [StringLength(5,MinimumLength = 5, ErrorMessage = "le code postal doit contenir exactement 5 caractères")]
    public string CodePostal { get; set; } = string.Empty;
    [Required]
    [StringLength(100,ErrorMessage ="Le nom de la ville ne doit pas dépasser 100 caractères")]
    public string Ville { get; set; } = string.Empty;
}

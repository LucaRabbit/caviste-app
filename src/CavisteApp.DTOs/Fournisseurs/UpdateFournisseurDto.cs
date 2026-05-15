using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CavisteApp.DTOs.Fournisseurs;

public class UpdateFournisseurDto
{
    [Required]
    public int Id { get; set; }
    [Required]
    [StringLength(100, ErrorMessage = "Le nom du vin ne peut pas dépasser 100 caractères.")]
    public string Nom { get; set; } = string.Empty;
    [Required]
    [StringLength(100, ErrorMessage = "Le nom du contact ne peut pas dépasser 100 caractères.")]
    [EmailAddress(ErrorMessage = "L'adresse e-mail n'est pas valide.")]
    public string Email { get; set; } = string.Empty;
    [Required]
    [StringLength(20, ErrorMessage = "Le numéro de téléphone ne peut pas dépasser 20 caractères.")]
    public string Telephone { get; set; } = string.Empty;
    [Required]
    public int NumRue { get; set; }
    [Required]
    [StringLength(200, ErrorMessage = "Le nom de la rue ne peut pas dépasser 200 caractères.")]
    public string NomRue { get; set; } = string.Empty;
    [Required]
    [StringLength(5, ErrorMessage = "Le code postal ne peut pas dépasser 5 caractères.")]
    public string CodePostal { get; set; } = string.Empty;
    [Required]
    [StringLength(100, ErrorMessage = "Le nom de la ville ne peut pas dépasser 100 caractères.")]
    public string Ville { get; set; } = string.Empty;
}

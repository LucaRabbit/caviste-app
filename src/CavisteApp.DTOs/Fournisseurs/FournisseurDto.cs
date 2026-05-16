using System;
using System.Collections.Generic;
using System.Text;

namespace CavisteApp.DTOs.Fournisseurs;

public class FournisseurDto
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telephone { get; set; } = string.Empty;
    public int NumRue { get; set; }
    public string NomRue { get; set; } = string.Empty;
    public string CodePostal { get; set; } = string.Empty;
    public string Ville { get; set; } = string.Empty;
}

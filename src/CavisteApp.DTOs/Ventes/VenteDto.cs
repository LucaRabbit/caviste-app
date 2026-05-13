using System;
using System.Collections.Generic;
using System.Text;

namespace CavisteApp.DTOs.Ventes;

public class VenteDto
{
    public int Id { get; set; }
    public DateTime Date{ get; set; }
    public decimal MontantTotal { get; set; }

    public int ClientId { get; set; }
    public string ClientNom { get; set; } = string.Empty;

    public int UtilisateurId { get; set; }
    public string UtilisateurNom { get; set; } = string.Empty;

    //public List<LigneVenteDto> Lignes { get; set; } = new();

}

public class CreerVenteRequest
{
    public int ClientId { get; set; }
    //public List<CreerLigneVenteDto> Lignes { get; set; } = new();
}

public class ModifierVenteRequest : CreerVenteRequest
{
    public int Id { get; set; }
}

// DTO résumé
public class VenteResumeDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public decimal MontantTotal { get; set; }
    public string ClientNom { get; set; } = string.Empty;
    //public int NombreLignes { get; set; }
}

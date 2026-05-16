using CavisteApp.Api.Constants;
using CavisteApp.Api.Data;
using CavisteApp.Api.Entities;
using CavisteApp.Api.Enums;
using CavisteApp.DTOs.Commandes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CavisteApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]

public class CommandesController : ControllerBase
{
    private readonly CavisteDbContext _context;

    public CommandesController(CavisteDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CommandeResumeDto>>> GetAll()
    {
        var commandes = await _context.Commandes
            .Include(c => c.Fournisseur)
            .OrderByDescending(c => c.DateCreation)
            .Select(c => new CommandeResumeDto
            {
                Id = c.Id,
                DateCreation = c.DateCreation,
                DateReception = c.DateReception,
                DateValidation = c.DateValidation,
                FournisseurNom = c.Fournisseur.Nom,
                Statut = (int)c.Statut,
                NombreLignes = c.Lignes.Count
            })
            .ToListAsync();

        if (commandes == null || commandes.Count == 0)
        {
            return BadRequest("Aucune commande trouvée.");
        }

        return Ok(commandes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CommandeDto>> GetById(int id)
    {
        var commande = await _context.Commandes
            .Include(c => c.Fournisseur)
            .Include(c => c.Lignes)
            .Where(c => c.Id == id)
            .Select(c => new CommandeDto
            {
                Id = c.Id,
                DateCreation = c.DateCreation,
                DateValidation = c.DateValidation,
                DateReception = c.DateReception,
                Statut = (int)c.Statut,
                FournisseurNom = c.Fournisseur.Nom,
                FournisseurId = c.Fournisseur.Id,
                Lignes = c.Lignes.Select(l => new LigneCommandeDto
                {
                    Id = l.Id,
                    VinId = l.VinId,
                    VinNom = l.Vin.Nom,
                    Quantite = l.Quantite,
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (commande == null)
        {
            return BadRequest($"La commande avec Id '{id}' n'existe pas.");
        }
        return Ok(commande);
    }

    [HttpPost]
    public async Task<ActionResult<CommandeDto>> Create([FromBody] CreerCommandeDto request)
    {
        // Vérifier si le fournisseur existe
        var fournisseur = await _context.Fournisseurs.FindAsync(request.FournisseurId);
        if (fournisseur == null)
        {
            return BadRequest($"Le fournisseur avec Id '{request.FournisseurId}' n'existe pas.");
        }

        // Vérifier si la commande contient des lignes
        if (request.Lignes is null || request.Lignes.Count == 0)
            return BadRequest("Une commande doit contenir au moins une ligne.");

        // Récuperer les vins des lignes
        var vinIds = request.Lignes.Select(l => l.VinId).Distinct().ToList();
        var vins = await _context.Vins
            .Where(v => vinIds.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id);

        // Vérifier que les vins existent sinon : Créer le nouveau vin avant de créer la commande et incrémenter le stock du vin

        foreach (var ligne in request.Lignes)
        {
            if (!vins.TryGetValue(ligne.VinId, out var vin))
                _context.Vins.Add(new Vin
                {
                    Id = ligne.VinId,
                    Nom = $"Vin {ligne.VinId}",
                    Type = TypeVin.Rouge,
                    Prix = 0,
                    Stock = 0,
                    SeuilStockBas = 5,
                    DateCreation = DateTime.UtcNow
                });
        }

        // Créer la commande
        var commande = new Commande
        {
            DateCreation = DateTime.UtcNow,
            Statut = StatutCommande.Brouillon,
            FournisseurId = request.FournisseurId,
            Lignes = request.Lignes.Select(l => new LigneCommande
            {
                VinId = l.VinId,
                Quantite = l.Quantite
            }).ToList()
        };

        // Incrementer le stock des vins
        foreach (var ligne in request.Lignes)
        {
            vins[ligne.VinId].Stock += ligne.Quantite;
        }

        // Enregistrer la commande
        _context.Commandes.Add(commande);
        await _context.SaveChangesAsync();

        // TODO: Recharger avec les relations pour retourner la vente complète
        await _context.Entry(commande).Reference(c => c.Fournisseur).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { id = commande.Id }, MapToDto(commande));
    }


    [HttpDelete("{id:int}")]
    [Authorize(Roles = RolesConstants.Administrateur)] // Contrôle de rôle Identity
    public async Task<IActionResult> DeleteCommande(int id)
    {
        var commande = await _context.Commandes.FindAsync(id);
        if (commande == null)
        {
            return BadRequest($"La commande avec Id '{id}' n'existe pas.");
        }

        // Suppression de la commande
        _context.Commandes.Remove(commande);

        // Sauvegarde
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static CommandeDto MapToDto(Commande commande)
    {
        return new CommandeDto
        {
            Id = commande.Id,
            DateCreation = commande.DateCreation,
            DateValidation = commande.DateValidation,
            DateReception = commande.DateReception,
            Statut = (int)commande.Statut,
            FournisseurId = commande.FournisseurId,
            FournisseurNom = commande.Fournisseur?.Nom ?? string.Empty,
            Lignes = commande.Lignes.Select(l => new LigneCommandeDto
            {
                Id = l.Id,
                VinId = l.VinId,
                Quantite = l.Quantite
            }).ToList()
        };
    }
}

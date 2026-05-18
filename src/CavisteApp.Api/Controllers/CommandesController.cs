using CavisteApp.Api.Constants;
using CavisteApp.Api.Data;
using CavisteApp.Api.Entities;
using CavisteApp.DTOs.Commandes;
using CavisteApp.DTOs.Enums;
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
    public async Task<ActionResult<IEnumerable<CommandeResumeDto>>> GetAll(
        [FromQuery] StatutCommande? statut = null, 
        [FromQuery] int? fournisseurId = null)
    {
        var query = _context.Commandes
            .Include(c => c.Fournisseur)
            .Include(c => c.Lignes)
            .AsQueryable();

        if (statut.HasValue)
            query = query.Where(c => c.Statut == statut.Value);

        if (fournisseurId.HasValue)
            query = query.Where(c => c.FournisseurId == fournisseurId.Value);

        var commandes = await query
            .OrderByDescending(c => c.DateCreation)
            .Select(c => new CommandeResumeDto
            {
                Id = c.Id,
                DateCreation = c.DateCreation,
                DateReception = c.DateReception,
                DateValidation = c.DateValidation,
                FournisseurNom = c.Fournisseur.Nom,
                Statut = c.Statut,
                NombreLignes = c.Lignes.Count
            })
            .ToListAsync();

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
                Statut = c.Statut,
                FournisseurNom = c.Fournisseur.Nom,
                FournisseurId = c.Fournisseur.Id,
                Lignes = c.Lignes.Select(l => new LigneCommandeDto
                {
                    Id = l.Id,
                    VinId = l.VinId,
                    VinNom = l.VinNom,
                    Quantite = l.Quantite,
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (commande == null)
        {
            return NotFound($"La commande avec Id '{id}' n'existe pas.");
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
            return NotFound($"Le fournisseur avec Id '{request.FournisseurId}' n'existe pas.");
        }

        // Vérifier si la commande contient des lignes
        if (request.Lignes is null || request.Lignes.Count == 0)
            return BadRequest("Une commande doit contenir au moins une ligne.");

        // Récuperer les vins des lignes
        var vinIds = request.Lignes.Select(l => l.VinId).Distinct().ToList();
        var vins = await _context.Vins
            .Where(v => vinIds.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id);

        // Vérifier que les vins des lignes existent
        foreach (var ligne in request.Lignes)
        {
            if (!vins.TryGetValue(ligne.VinId, out var vin))
            {
                return NotFound($"Le vin avec Id {ligne.VinId} n'existe pas."); 
            }
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

        // Enregistrer la commande
        _context.Commandes.Add(commande);
        await _context.SaveChangesAsync();

        // Recharger avec les relations pour retourner la vente complète
        await _context.Entry(commande).Reference(c => c.Fournisseur).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { id = commande.Id }, MapToDto(commande));
    }

    // PUT: api/commandes/5 - seulement si brouillon
    [HttpPut("{id:int}")]
    [Authorize(Roles = RolesConstants.Administrateur)]
    public async Task<ActionResult<CommandeDto>> Update(int id, [FromBody] UpdateCommandeDto request)
    {
        var commande = await _context.Commandes
            .Include(c => c.Fournisseur)
            .Include(c => c.Lignes)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (commande is null)
            return NotFound($"La commande {id} n'existe pas.");

        // Règle métier : seules les commandes en brouillon sont modifiables
        if (commande.Statut != StatutCommande.Brouillon)
            return Conflict(
                $"Seules les commandes en brouillon peuvent être modifiées (statut actuel : {commande.Statut}). " +
                "Annulez cette commande et créez-en une nouvelle si nécessaire.");

        // Vérifier que le fournisseur existe
        var fournisseurExiste = await _context.Fournisseurs.AnyAsync(f => f.Id == request.FournisseurId);
        if (!fournisseurExiste)
            return NotFound($"Le fournisseur {request.FournisseurId} n'existe pas.");

        // Vérifier que les vins existent
        var vinIds = request.Lignes.Select(l => l.VinId).Distinct().ToList();
        var vins = await _context.Vins
            .Where(v => vinIds.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id);

        foreach (var ligne in request.Lignes)
        {
            if (!vins.ContainsKey(ligne.VinId))
                return NotFound($"Le vin {ligne.VinId} n'existe pas.");
        }

        // Mise à jour des champs simples
        commande.FournisseurId = request.FournisseurId;

        // Réconciliation des lignes : identifier les lignes à supprimer, modifier, ajouter
        var idsLignesEnvoyees = request.Lignes
            .Where(l => l.Id.HasValue)
            .Select(l => l.Id!.Value)
            .ToHashSet();

        // Supprimer les lignes qui ne sont plus présentes
        var lignesASupprimer = commande.Lignes
            .Where(l => !idsLignesEnvoyees.Contains(l.Id))
            .ToList();

        foreach (var ligne in lignesASupprimer)
            commande.Lignes.Remove(ligne);

        // Modifier les lignes existantes et ajouter les nouvelles
        foreach (var ligneDto in request.Lignes)
        {
            var vin = vins[ligneDto.VinId];

            if (ligneDto.Id.HasValue)
            {
                // Modification d'une ligne existante
                var ligneExistante = commande.Lignes.FirstOrDefault(l => l.Id == ligneDto.Id.Value);
                if (ligneExistante is null)
                    return BadRequest($"La ligne {ligneDto.Id} n'appartient pas à cette commande.");

                ligneExistante.VinId = ligneDto.VinId;
                ligneExistante.VinNom = vin.Nom;
                ligneExistante.VinType = vin.Type;
                ligneExistante.Quantite = ligneDto.Quantite;
            }
            else
            {
                // Nouvelle ligne
                commande.Lignes.Add(new LigneCommande
                {
                    VinId = ligneDto.VinId,
                    VinNom = vin.Nom,
                    VinType = vin.Type,
                    Quantite = ligneDto.Quantite,
                });
            }
        }

        await _context.SaveChangesAsync();

        return Ok(MapToDto(commande));
    }

    // DELETE: api/commandes/5 - seulement si brouillon
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCommande(int id)
    {
        var commande = await _context.Commandes.FindAsync(id);
        if (commande == null)
        {
            return NotFound($"La commande avec Id '{id}' n'existe pas.");
        }

        if (commande.Statut != StatutCommande.Brouillon)
        {
            return Conflict(
                $"Seules les commandes au statut 'Brouillon' peuvent être supprimées (statut actuel : {commande.Statut})." +
                "Utiliser l'annulation pour les commandes en cours.");
        }

        // Suppression de la commande
        _context.Commandes.Remove(commande);

        // Sauvegarde
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // POST api/commandes/{id}/valider
    [HttpPost("{id:int}/valider")]
    [Authorize(Roles = RolesConstants.Administrateur)] // Contrôle de rôle Identity
    public async Task<ActionResult<CommandeDto>> Valider(int id)
    {
        var commande = await _context.Commandes
            .Include(c => c.Fournisseur)
            .Include(c => c.Lignes)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (commande == null)
        {
            return NotFound($"La commande avec Id '{id}' n'existe pas.");
        }

        if (commande.Statut != StatutCommande.Brouillon)
        {
            return Conflict("Seules les commandes au statut 'Brouillon' peuvent être validées.");
        }

        if (commande.Lignes.Count == 0)
        {
            return BadRequest("Impossible de valider une commande sans ligne.");
        }

        // Valider la commande
        commande.Statut = StatutCommande.Validee;
        commande.DateValidation = DateTime.UtcNow;

        // Sauvegarder les modifications
        await _context.SaveChangesAsync();
        return Ok(MapToDto(commande));
    }

    // POST: api/commandes/{id}/receptionner
    [HttpPost("{id:int}/receptionner")]
    [Authorize(Roles = RolesConstants.Administrateur)] // Contrôle de rôle Identity
    public async Task<ActionResult<CommandeDto>> Receptionner(int id, [FromBody] ReceptionnerCommandeDto request)
    {
        var commande = await _context.Commandes
            .Include(c => c.Fournisseur)
            .Include(c => c.Lignes)
            .ThenInclude(l => l.Vin)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (commande == null)
        {
            return NotFound($"La commande avec Id '{id}' n'existe pas.");
        }

        if (commande.Statut != StatutCommande.Validee)
        {
            return Conflict("Seules les commandes au statut 'Validée' peuvent être réceptionnées.");
        }

        if (request.Lignes is null || request.Lignes.Count == 0)
        {
            return BadRequest("La réception d'une commande doit contenir au moins une ligne.");
        }

        // Récuperer les vins des lignes
        var quantitesRecues = request.Lignes.ToDictionary(l => l.Id, l => l.QuantiteRecue);

        // Vérifier que les lignes de la réception correspondent bien aux lignes de la commande
        var idsLignesCommande = commande.Lignes.Select(l => l.Id).ToHashSet();
        var idsInvalides = quantitesRecues.Keys.Where(id => !idsLignesCommande.Contains(id)).ToList();
        if (idsInvalides.Count > 0)
        {
            return BadRequest($"Les lignes de commande avec Id suivants sont invalides et n'appartiennent pas à cette commande : {string.Join(", ", idsInvalides)}");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var ligne in commande.Lignes)
            {
                var quantiteRecue = quantitesRecues.TryGetValue(ligne.Id, out var qr) ? qr : ligne.Quantite;

                ligne.QuantiteRecue = quantiteRecue;

                if (quantiteRecue > 0)
                {
                    ligne.Vin.Stock += quantiteRecue;
                }
            }

            commande.Statut = StatutCommande.Receptionnee;
            commande.DateReception = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        return Ok(MapToDto(commande));
    }

    // POST: api/commandes/{id}/annuler
    [HttpPost]
    [Route("{id:int}/annuler")]
    [Authorize(Roles = RolesConstants.Administrateur)] // Contrôle de rôle Identity
    public async Task<ActionResult<CommandeDto>> Annuler(int id, [FromBody] AnnulerCommandeDto request)
    {
        var commande = await _context.Commandes
            .Include(c => c.Fournisseur)
            .Include(c => c.Lignes)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (commande == null)
        {
            return NotFound($"La commande avec Id '{id}' n'existe pas.");
        }

        if (commande.Statut == StatutCommande.Receptionnee)
        {
            return Conflict("Une commande réceptionnée ne peut pas être annulée.");
        }

        if (commande.Statut == StatutCommande.Annulee)
        {
            return Conflict("La commande est déjà annulée.");
        }

        commande.Statut = StatutCommande.Annulee;
        // TODO: Stocker le motif d'annulation dans un champ dédié si besoin (non présent dans l'entité actuelle)

        await _context.SaveChangesAsync();
        return Ok(MapToDto(commande));
    }

    // Méthode de mapping de Commande vers CommandeDto
    private static CommandeDto MapToDto(Commande commande)
    {
        return new CommandeDto
        {
            Id = commande.Id,
            DateCreation = commande.DateCreation,
            DateValidation = commande.DateValidation,
            DateReception = commande.DateReception,
            Statut = commande.Statut,
            FournisseurId = commande.FournisseurId,
            FournisseurNom = commande.Fournisseur?.Nom ?? string.Empty,
            Lignes = commande.Lignes.Select(l => new LigneCommandeDto
            {
                Id = l.Id,
                VinId = l.VinId,
                VinNom = l.VinNom,
                Quantite = l.Quantite,
                QuantiteRecue = l.QuantiteRecue,
            }).ToList()
        };
    }
}

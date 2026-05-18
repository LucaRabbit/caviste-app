using CavisteApp.Api.Constants;
using CavisteApp.Api.Data;
using CavisteApp.Api.Dtos.Ventes;
using CavisteApp.Api.Entities;
using CavisteApp.Api.Services.Stock;
using CavisteApp.DTOs.Enums;
using CavisteApp.DTOs.Ventes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CavisteApp.Api.Services.QuestPDF;

namespace CavisteApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VentesController : ControllerBase
{
    private readonly QuestPdfService _pdfService;
    private readonly CavisteDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AlerteStockService _alerteStock;

    public VentesController(CavisteDbContext context, UserManager<ApplicationUser> userManager, AlerteStockService alerteStock, QuestPdfService pdfService)
    {
        _context = context;
        _userManager = userManager;
        _alerteStock = alerteStock;
        _pdfService = pdfService;
    }

    // GET api/vins
    [HttpGet]
    public async Task<ActionResult<IEnumerable<VenteResumeDto>>> GetAll(
        [FromQuery] StatutVente? statut = null,
        [FromQuery] int? clientId = null)
    {
        var query = _context.Ventes
            .Include(v => v.Client)
            .Include(v => v.Lignes)
            .AsQueryable();

        if (statut.HasValue)
            query = query.Where(v => v.Statut == statut.Value);

        if (clientId.HasValue)
            query = query.Where(v => v.ClientId == clientId.Value);

        var ventes = await query
            .OrderByDescending(v => v.Date)
            .Select(v => new VenteResumeDto
            {
                Id = v.Id,
                Date = v.Date,
                MontantTotal = v.MontantTotal,
                Statut = v.Statut,
                ClientNom = v.Client.Nom,
                NombreLignes = v.Lignes.Count
            })
            .ToListAsync();

        return Ok(ventes);
    }

    // GET api/ventes/5
    [HttpGet("{id}")]
    public async Task<ActionResult<VenteDto>> GetById(int id)
    {
        var vente = await _context.Ventes
            .Include(v => v.Client)
            .Include(v => v.Utilisateur)
            .Include(v => v.Lignes)
                .ThenInclude(l => l.Vin)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vente == null)
        {
            return NotFound($"La vente avec Id '{id}' n'existe pas.");
        }

        return Ok(MapToDto(vente));
    }

    // POST api/ventes
    [HttpPost]
    public async Task<ActionResult<VenteDto>> Create([FromBody] CreerVenteDto request)
    {
        // Récupérer l'utilisateur authentifié
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // Vérifier si le client existe
        var client = await _context.Clients.FindAsync(request.ClientId);
        if (client == null)
        {
            return NotFound($"Le client avec Id '{request.ClientId}' n'existe pas.");
        }

        // Vérifier si la vente contient des lignes
        if (request.Lignes is null || request.Lignes.Count == 0)
            return BadRequest("Une vente doit contenir au moins une ligne.");

        // Récuperer les vins des lignes
        var vinIds = request.Lignes.Select(l => l.VinId).Distinct().ToList();
        var vins = await _context.Vins
            .Where(v => vinIds.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id);

        // Vérifier que les vins existent et ont stock suffisant avant de créer la vente
        foreach (var ligne in request.Lignes)
        {
            if (!vins.TryGetValue(ligne.VinId, out var vin))
            { 
                return NotFound($"Le vin {ligne.VinId} n'existe pas."); 
            }
        }

        // Créer la vente
        var vente = new Vente
        {
            Date = DateTime.UtcNow,
            Statut = StatutVente.Brouillon,
            ClientId = request.ClientId,
            UtilisateurId = int.Parse(userId),
            Lignes = request.Lignes.Select(l =>
            { 
                var vin = vins[l.VinId];
                return new LigneVente
                {
                    VinId = vin.Id,
                    VinNom = vin.Nom,
                    Type = vin.Type,
                    Quantite = l.Quantite,
                    PrixUnitaire = l.PrixUnitaire
                };
            }).ToList()
        };



        // Calculer le montant total de la vente
        vente.CalculerMontantTotal();

        // Enregistrer la vente
        _context.Ventes.Add(vente);
        await _context.SaveChangesAsync();

        // Recharger avec les relations pour retourner la vente complète
        await _context.Entry(vente).Reference(v => v.Client).LoadAsync();
        await _context.Entry(vente).Reference(v => v.Utilisateur).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { id = vente.Id }, MapToDto(vente));
    }

    // PUT api/ventes/5 - seulement si brouillon
    [HttpPut("{id:int}")]
    public async Task<ActionResult<VenteDto>> Update(int id, [FromBody] UpdateVenteDto request)
    {
        var vente = await _context.Ventes
            .Include(v => v.Client)
            .Include(v => v.Utilisateur)
            .Include(v => v.Lignes)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vente is null)
            return NotFound();

        // Seules les ventes en brouillon sont modifiables
        if (vente.Statut != StatutVente.Brouillon)
            return Conflict(
                $"Seules les ventes en brouillon peuvent être modifiées (statut actuel : {vente.Statut}).");

        // Validation client + vins
        var clientExiste = await _context.Clients.AnyAsync(c => c.Id == request.ClientId);
        if (!clientExiste)
            return NotFound($"Le client {request.ClientId} n'existe pas.");

        var vinIds = request.Lignes.Select(l => l.VinId).Distinct().ToList();
        var vins = await _context.Vins
            .Where(v => vinIds.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id);

        foreach (var ligne in request.Lignes)
        {
            if (!vins.ContainsKey(ligne.VinId))
            {
                return NotFound($"Le vin {ligne.VinId} n'existe pas."); 
            }
        }

        vente.ClientId = request.ClientId;

        // Réconciliation des lignes : identifier les lignes à supprimer, modifier, ajouter
        var idsEnvoyes = request.Lignes
            .Where(l => l.Id.HasValue)
            .Select(l => l.Id!.Value)
            .ToHashSet();

        // Supprimer les lignes qui ne sont plus présentes
        var aSupprimer = vente.Lignes.Where(l => !idsEnvoyes.Contains(l.Id)).ToList();

        foreach (var ligne in aSupprimer)
            vente.Lignes.Remove(ligne);

        // Modifier les lignes existantes et ajouter les nouvelles
        foreach (var dtoLigne in request.Lignes)
        {
            var vin = vins[dtoLigne.VinId];

            if (dtoLigne.Id.HasValue)
            {
                // Modification d'une ligne existante
                var existante = vente.Lignes.FirstOrDefault(l => l.Id == dtoLigne.Id.Value);
                if (existante is null)
                    return BadRequest($"La ligne {dtoLigne.Id} n'appartient pas à cette vente.");

                existante.VinId = vin.Id;
                existante.VinNom = vin.Nom;
                existante.Type = vin.Type;
                existante.Quantite = dtoLigne.Quantite;
                existante.PrixUnitaire = dtoLigne.PrixUnitaire;
            }
            else
            {
                vente.Lignes.Add(new LigneVente
                {
                    VinId = vin.Id,
                    VinNom = vin.Nom,
                    Type = vin.Type,
                    Quantite = dtoLigne.Quantite,
                    PrixUnitaire = dtoLigne.PrixUnitaire
                });
            }
        }

        vente.CalculerMontantTotal();
        await _context.SaveChangesAsync();

        return Ok(MapToDto(vente));
    }


    // DELETE api/ventes/5 - seulement si brouillon
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteVente(int id)
    {
        var vente = await _context.Ventes.FindAsync(id);

        if (vente == null)
        {
            return NotFound($"La vente avec Id '{id}' n'existe pas.");
        }

        if (vente.Statut != StatutVente.Brouillon)
        {
            return Conflict(
                $"Seules les ventes en brouillon peuvent être supprimées (statut actuel : {vente.Statut}). " +
                "Utilisez l'annulation pour les ventes validées.");
        }

        // Suppression de la vente
        _context.Ventes.Remove(vente);

        // Sauvegarde
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // POST: api/ventes/5/valider - diminue les stocks, vérifie les alertes, change le statut
    [HttpPost("{id:int}/valider")]
    public async Task<ActionResult<VenteDto>> Valider(int id)
    {
        var vente = await _context.Ventes
            .Include(v => v.Client)
            .Include(v => v.Utilisateur)
            .Include(v => v.Lignes)
                .ThenInclude(l => l.Vin)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vente is null)
        {  
            return NotFound();
        }

        if (vente.Statut != StatutVente.Brouillon)
        {
            return Conflict(
                $"Seules les ventes en brouillon peuvent être validées (statut actuel : {vente.Statut}).");
        }

        if (vente.Lignes.Count == 0)
            return BadRequest("Impossible de valider une vente sans ligne.");

        // Vérification du stock au moment de la validation
        foreach (var ligne in vente.Lignes)
        {
            if (ligne.Vin.Stock < ligne.Quantite)
                return Conflict(
                    $"Stock insuffisant pour '{ligne.VinNom}' (disponible : {ligne.Vin.Stock}, demandé : {ligne.Quantite}).");
        }

        // Transaction : décrément stock + changement de statut
        var stocksAvant = new Dictionary<int, int>();

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var ligne in vente.Lignes)
            {
                var vin = ligne.Vin;

                // On capture une seule fois par vin (au cas où le même vin apparaît sur plusieurs lignes)
                if (!stocksAvant.ContainsKey(vin.Id))
                    stocksAvant[vin.Id] = vin.Stock;

                vin.Stock -= ligne.Quantite;
            }

            vente.Statut = StatutVente.Validee;
            vente.DateValidation = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        var pdfBytes = _pdfService.GenererTicketPdf(vente);
        var cheminPdf = _pdfService.SauvegarderTicketPdf(vente, "C:\\Users\\lucar\\Downloads");

        // Alerte de stock bas
        foreach (var (vinId, stockAvant) in stocksAvant)
        {
            var vin = vente.Lignes.First(l => l.VinId == vinId).Vin;
            await _alerteStock.VerifierEtAlerterAsync(vin, stockAvant);
        }

        return Ok(MapToDto(vente));
    }

    // POST: api/ventes/5/annuler — restaure le stock si la vente était validée
    [HttpPost("{id:int}/annuler")]
    [Authorize(Roles = RolesConstants.Administrateur)] // Contrôle de rôle Identity
    public async Task<ActionResult<VenteDto>> Annuler(int id, [FromBody] AnnulerVenteDto dto)
    {
        var vente = await _context.Ventes
            .Include(v => v.Client)
            .Include(v => v.Utilisateur)
            .Include(v => v.Lignes)
                .ThenInclude(l => l.Vin)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vente is null)
            return NotFound();

        if (vente.Statut == StatutVente.Annulee)
            return Conflict("Cette vente est déjà annulée.");

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Si vente était validée, restauration du stock
            if (vente.Statut == StatutVente.Validee)
            {
                foreach (var ligne in vente.Lignes)
                {
                    ligne.Vin.Stock += ligne.Quantite;
                }
            }

            vente.Statut = StatutVente.Annulee;
            vente.DateAnnulation = DateTime.UtcNow;
            vente.MotifAnnulation = dto.Motif;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        return Ok(MapToDto(vente));
    }

    // Mapping Dto
    private static VenteDto MapToDto(Vente vente)
    {
        return new VenteDto
        {
            Id = vente.Id,
            Date = vente.Date,
            MontantTotal = vente.MontantTotal,
            Statut = vente.Statut,
            DateValidation = vente.DateValidation,
            ClientId = vente.ClientId,
            ClientNom = vente.Client?.Nom ?? string.Empty,
            UtilisateurId = vente.UtilisateurId,
            UtilisateurNom = vente.Utilisateur?.UserName ?? string.Empty,
            Lignes = vente.Lignes.Select(l => new LigneVenteDto
            {
                Id = l.Id,
                VinId = l.VinId,
                VinNom = l.VinNom,
                Type = l.Type,
                Quantite = l.Quantite,
                PrixUnitaire = l.PrixUnitaire
            }).ToList()
        };
    }
}

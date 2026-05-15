using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CavisteApp.Api.Data;
using CavisteApp.Api.Entities;
using CavisteApp.DTOs.Ventes;
using Microsoft.AspNetCore.Identity;
using CavisteApp.Api.Constants;

namespace CavisteApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VentesController : ControllerBase
{
    private readonly CavisteDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public VentesController(CavisteDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET api/vins
    [HttpGet]
    public async Task<ActionResult<IEnumerable<VenteResumeDto>>> GetAll()
    {
        var ventes = await _context.Ventes
            .Include(v => v.Client)
            .OrderByDescending(v => v.Date)
            .Select(v => new VenteResumeDto
            {
                Id = v.Id,
                Date = v.Date,
                MontantTotal = v.MontantTotal,
                ClientNom = v.Client.Nom,
                NombreLignes = v.Lignes.Count
            })
            .ToListAsync();

        if (ventes == null || ventes.Count == 0)
        {
            return BadRequest("Aucune vente trouvée.");
        }

        return Ok(ventes);
    }

    // GET api/ventes/5
    [HttpGet("{id}")]
    public async Task<ActionResult<VenteDto>> GetById(int id)
    {
        var vente = await _context.Ventes
            .Include(v => v.Client)
            .Include(v => v.Lignes)
            .Where(v => v.Id == id)
            .Select(v => new VenteDto
            {
                Id = v.Id,
                Date = v.Date,
                MontantTotal = v.MontantTotal,
                ClientId = v.ClientId,
                ClientNom = v.Client.Nom,
                Lignes = v.Lignes.Select(l => new LigneVenteDto
                {
                    Id = l.Id,
                    VinId = l.VinId,
                    VinNom = l.Vin.Nom,
                    Quantite = l.Quantite,
                    PrixUnitaire = l.PrixUnitaire
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (vente == null)
        {
            return BadRequest($"La vente avec Id '{id}' n'existe pas.");
        }
        return Ok(vente);
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
            return BadRequest($"Le client avec Id '{request.ClientId}' n'existe pas.");
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
                return BadRequest($"Le vin {ligne.VinId} n'existe pas.");

            if (vin.Stock < ligne.Quantite)
                return BadRequest($"Stock insuffisant pour 'vin.Nom' (Stock disponible : {vin.Stock}).");
        }

        // Créer la vente
        var vente = new Vente
        {
            Date = DateTime.UtcNow,
            ClientId = request.ClientId,
            UtilisateurId = int.Parse(userId),
            Lignes = request.Lignes.Select(l => new LigneVente
            {
                VinId = l.VinId,
                Quantite = l.Quantite,
                PrixUnitaire = l.PrixUnitaire
            }).ToList()
        };

        // Decrementer le stock des vins
        foreach (var ligne in request.Lignes)
        {
            vins[ligne.VinId].Stock -= ligne.Quantite;
        }

        // Calculer le montant total de la vente
        vente.CalculerMontantTotal();

        // Enregistrer la vente
        _context.Ventes.Add(vente);
        await _context.SaveChangesAsync();

        // TODO: Recharger avec les relations pour retourner la vente complète
        await _context.Entry(vente).Reference(v => v.Client).LoadAsync();
        await _context.Entry(vente).Reference(v => v.Utilisateur).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { id = vente.Id }, MapToDto(vente));
    }

    // DELETE api/ventes/5
    [HttpDelete("{id:int}")]
    [Authorize(Roles = RolesConstants.Administrateur)] // Contrôle de rôle Identity
    public async Task<IActionResult> DeleteVente(int id)
    {
        var vente = await _context.Ventes.FindAsync(id);
        if (vente == null)
        {
            return BadRequest($"La vente avec Id '{id}' n'existe pas.");
        }

        // Suppression de la vente
        _context.Ventes.Remove(vente);

        // Sauvegarde
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // Mapping Dto
    private static VenteDto MapToDto(Vente vente)
    {
        return new VenteDto
        {
            Id = vente.Id,
            Date = vente.Date,
            MontantTotal = vente.MontantTotal,
            ClientId = vente.ClientId,
            ClientNom = vente.Client?.Nom ?? string.Empty,
            UtilisateurId = vente.UtilisateurId,
            UtilisateurNom = vente.Utilisateur?.UserName ?? string.Empty,
            Lignes = vente.Lignes.Select(l => new LigneVenteDto
            {
                Id = l.Id,
                VinId = l.VinId,
                Quantite = l.Quantite,
                PrixUnitaire = l.PrixUnitaire
            }).ToList()
        };
    }
}

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
            return NotFound();
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

        var client = await _context.Clients.FindAsync(request.ClientId);
        if (client == null)
        {
            return BadRequest($"Client avec Id {request.ClientId} non trouvé.");
        }

        // TODO: Ajouter vérification que vins existent et ont stock suffisant avant de créer la vente

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

        // TODO: Decrementer le stock des vins et calculer le montant total de la vente

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
            return NotFound();
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
            ClientNom = vente.Client.Nom,
            Lignes = vente.Lignes.Select(l => new LigneVenteDto
            {
                Id = l.Id,
                VinId = l.VinId,
                VinNom = l.Vin.Nom,
                Quantite = l.Quantite,
                PrixUnitaire = l.PrixUnitaire
            }).ToList()
        };
    }
}

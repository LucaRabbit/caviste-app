using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using CavisteApp.Api.Constants;
using CavisteApp.Api.Data;
using CavisteApp.Api.Entities;
using CavisteApp.DTOs.Fournisseurs;

namespace CavisteApp.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FournisseursController : ControllerBase
{
    private readonly CavisteDbContext _context;

    public FournisseursController(CavisteDbContext context)
    {
        _context = context;
    }

    // GET: api/fournisseurs
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FournisseurDto>>> GetAll()
    {
        var fournisseurs = await _context.Fournisseurs
            .AsNoTracking()
            .OrderBy(f => f.Id)
            .ToListAsync();

        return Ok(fournisseurs.Select(MapToDto));

    }

    // GET: api/fournisseurs/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<FournisseurDto>> GetById(int id)
    {
        var fournisseur = await _context.Fournisseurs
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id);

        if (fournisseur == null)
        {
            return NotFound($"Le fournisseur avec Id '{id}' n'existe pas.");
        }

        return Ok(MapToDto(fournisseur));
    }

    // POST: api/fournisseurs
    [HttpPost]
    public async Task<ActionResult<FournisseurDto>> Create([FromBody] CreerFournisseurDto request)
    {
        var fournisseur = new Fournisseur
        {
            Nom = request.Nom,
            Email = request.Email,
            Telephone = request.Telephone,
            NumRue = request.NumRue,
            NomRue = request.NomRue,
            CodePostal = request.CodePostal,
            Ville = request.Ville
        };
        _context.Fournisseurs.Add(fournisseur);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = fournisseur.Id }, MapToDto(fournisseur));
    }

    // PUT: api/fournisseurs/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<FournisseurDto>> Update(int id, [FromBody] UpdateFournisseurDto request)
    {
        var fournisseur = await _context.Fournisseurs.FindAsync(id);

        if (fournisseur == null)
        {
            return NotFound($"Le fournisseur avec Id '{id}' n'existe pas.");
        }

        fournisseur.Nom = request.Nom;
        fournisseur.Email = request.Email;
        fournisseur.Telephone = request.Telephone;
        fournisseur.NumRue = request.NumRue;
        fournisseur.NomRue = request.NomRue;
        fournisseur.CodePostal = request.CodePostal;
        fournisseur.Ville = request.Ville;

        await _context.SaveChangesAsync();
        return Ok(MapToDto(fournisseur));
    }

    // DELETE: api/fournisseurs/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var fournisseur = await _context.Fournisseurs.FindAsync(id);

        if (fournisseur == null)
        {
            return NotFound($"Le fournisseur avec Id '{id}' n'existe pas.");
        }

        _context.Fournisseurs.Remove(fournisseur);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // Méthode de mapping pour convertir une entité Fournisseur en DTO
    private FournisseurDto MapToDto(Fournisseur fournisseur)
    {
        return new FournisseurDto
        {
            Id = fournisseur.Id,
            Nom = fournisseur.Nom,
            Email = fournisseur.Email,
            Telephone = fournisseur.Telephone,
            NumRue = fournisseur.NumRue,
            NomRue = fournisseur.NomRue,
            CodePostal = fournisseur.CodePostal,
            Ville = fournisseur.Ville
        };
    }
}

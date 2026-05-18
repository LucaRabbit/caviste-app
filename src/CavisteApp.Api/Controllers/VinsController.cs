using CavisteApp.Api.Constants;
using CavisteApp.Api.Data;
using CavisteApp.Api.Entities;
using CavisteApp.DTOs.Enums;
using CavisteApp.DTOs.Vins;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CavisteApp.Api.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VinsController : ControllerBase
    {

        private readonly CavisteDbContext _context;

        public VinsController(CavisteDbContext context)
        {
            _context = context;
        }

        // GET: api/
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VinDto>>> GetAll(
            [FromQuery] string? nom = null,
            [FromQuery] TypeVin? type = null,
            [FromQuery] bool? stockBas = null)
        {
            var query = _context.Vins.
                AsNoTracking().
                AsQueryable();

            if (!string.IsNullOrWhiteSpace(nom))
                query = query.Where(v => v.Nom.Contains(nom));

            if (type.HasValue)
                query = query.Where(v => v.Type == type.Value);

            if (stockBas == true)
                query = query.Where(v => v.Stock <= v.SeuilStockBas);

            var vins = await query.OrderBy(v => v.Nom).ToListAsync();
            return Ok(vins.Select(MapToDto));
        }

        // GET: api/vins/5
        [HttpGet("{id}")]
        public async Task<ActionResult<VinDto>> GetById(int id)
        {
            var vin = await _context.Vins
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vin == null)
            {
                return BadRequest($"Le vin avec Id '{id}' n'existe pas.");
            }

            return Ok(MapToDto(vin));
        }

        // POST: api/vins
        [HttpPost]
        public async Task<ActionResult<VinDto>> Create([FromBody] CreerVinDto request)
        {
            var vin = new Vin
            {
                Nom = request.Nom,
                Type = request.Type,
                Stock = request.StockInitial,
                SeuilStockBas = request.SeuilStockBas,
                Prix = request.Prix,
                DateCreation = DateTime.UtcNow
            };

            _context.Vins.Add(vin);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = vin.Id }, MapToDto(vin));
        }

        // PUT: api/vins/5
        [HttpPut("{id}")]
        [Authorize(Roles = RolesConstants.Administrateur)] // Contrôle de rôle Identity
        public async Task<IActionResult> Update(int id, [FromBody] UpdateVinDto request)
        {
            var vin = await _context.Vins.FindAsync(id);

            if (vin == null)
            {
                return BadRequest($"Le vin avec Id '{id}' n'existe pas.");
            }

            vin.Nom = request.Nom;
            vin.Type = (TypeVin)request.Type;
            vin.Prix = request.Prix;
            vin.SeuilStockBas = request.SeuilStockBas;

            await _context.SaveChangesAsync();
            return Ok(MapToDto(vin));
        }

        // DELETE: api/vins/5
        [HttpDelete("{id}")]
        [Authorize(Roles = RolesConstants.Administrateur)] // Contrôle de rôle Identity
        public async Task<IActionResult> DeleteVin(int id)
        {
            var vin = await _context.Vins.FindAsync(id);

            if (vin == null)
            {
                return BadRequest($"Le vin avec Id '{id}' n'existe pas.");
            }

            // Vérifier les relations avec les lignes de vente et de commande
            var estUtilise = await _context.Set<LigneVente>().AnyAsync(li => li.VinId == id) ||
                            await _context.Set<LigneCommande>().AnyAsync(li => li.VinId == id);

            // Si le vin est utilisé dans des lignes de vente ou de commande, empêcher la suppression
            if (estUtilise)
            {
                return BadRequest($"Le vin avec Id '{id}' ne peut pas être supprimé car il est utilisé dans des ventes ou des commandes.");
            }

            _context.Vins.Remove(vin);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/vins/5/reception
        [HttpPost("{id}/reception")]
        [Authorize(Roles = RolesConstants.Administrateur)] // Contrôle de rôle Identity
        public async Task<IActionResult> ReceptionStock(int id, [FromBody] ReceptionStockDto request)
        {
            var vin = await _context.Vins.FindAsync(id);

            if (vin == null)
            {
                return BadRequest($"Le vin avec Id '{id}' n'existe pas.");
            }

            vin.Stock += request.Quantite;

            await _context.SaveChangesAsync();

            return Ok(MapToDto(vin));
        }

        // POST: api/vins/5/retrait
        [HttpPost("{id}/retrait")]
        [Authorize(Roles = RolesConstants.Administrateur)] // Contrôle de rôle Identity
        public async Task<IActionResult> RetraitStock(int id, [FromBody] RetraitStockDto request)
        {
            var vin = await _context.Vins.FindAsync(id);

            if (vin == null)
            {
                return BadRequest($"Le vin avec Id '{id}' n'existe pas.");
            }
            if (request.Quantite > vin.Stock)
            {
                return BadRequest($"Le retrait de {request.Quantite} unités dépasse le stock actuel de {vin.Stock}.");
            }

            vin.Stock -= request.Quantite;

            await _context.SaveChangesAsync();
            return Ok(MapToDto(vin));
        }

        // POST: api/vins/5/inventaire
        [HttpPost("{id}/inventaire")]
        [Authorize(Roles = RolesConstants.Administrateur)] // Contrôle de rôle Identity
        public async Task<IActionResult> EnregistrerInventaire(int id, [FromBody] InventaireDto request)
        {
            var vin = await _context.Vins.FindAsync(id);

            if (vin == null)
            {
                return BadRequest($"Le vin avec Id '{id}' n'existe pas.");
            }

            vin.Stock = request.StockReel;

            await _context.SaveChangesAsync();
            return Ok(MapToDto(vin));
        }

        // Méthode de mapping pour convertir une entité Vin en VinDto
        private static VinDto MapToDto(Vin vin)
        {
            return new VinDto
            {
                Id = vin.Id,
                Nom = vin.Nom,
                Type = vin.Type,
                Prix = vin.Prix,
                Stock = vin.Stock,
                SeuilStockBas = vin.SeuilStockBas,
                CreatedDate = vin.DateCreation,
            };
        }

    }
}

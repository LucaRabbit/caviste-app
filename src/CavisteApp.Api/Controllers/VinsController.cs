using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using CavisteApp.Api.Constants;
using CavisteApp.Api.Data;
using CavisteApp.Api.Entities;
using CavisteApp.DTOs.Vins;

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
        public async Task<ActionResult<IEnumerable<VinDto>>> GetAll()
        {
            var vins =  await _context.Vins
                .AsNoTracking()
                .OrderBy(v => v.Id)
                .ToListAsync();

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
                Type = (Enums.TypeVin)request.Type,
                Stock = request.Stock,
                SeuilStockBas = request.SeuilStockBas,
                Prix = request.Prix
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
            var vin = await _context.Vins
                .Where(v => v.Id == id)
                .Select(v => new Vin
                {
                    Id = v.Id,
                    Nom = request.Nom,
                    Type = (Enums.TypeVin)request.Type,
                    Stock = request.Stock,
                    SeuilStockBas = request.SeuilStockBas,
                    Prix = request.Prix
                })
                .FirstOrDefaultAsync();

            if (vin == null)
            {
                return BadRequest($"Le vin avec Id '{id}' n'existe pas.");
            }
            
            await _context.SaveChangesAsync();
            return NoContent();
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

        private static VinDto MapToDto(Vin vin)
        {
            return new VinDto
            {
                Id = vin.Id,
                Nom = vin.Nom,
                Type = (int)vin.Type,
                Prix = vin.Prix,
                Stock = vin.Stock,
                SeuilStockBas = vin.SeuilStockBas,
                CreatedDate = vin.DateCreation
            };
        }

    }
}

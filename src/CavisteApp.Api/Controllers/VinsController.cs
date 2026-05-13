using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
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
            var vin =  await _context.Vins
                .Include(v => v.Type)
                .ToListAsync();

            return Ok(vin);
        }

        // GET: api/vins/5
        [HttpGet("{id}")]
        public async Task<ActionResult<VinDto>> GetById(int id)
        {
            var vin = await _context.Vins
                .Include(v => v.Type)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vin == null)
            {
                return NotFound();
            }

            return Ok(vin);
        }

        // POST: api/vins
        [HttpPost]
        public async Task<ActionResult<VinDto>> Create(Vin vin)
        {
            _context.Vins.Add(vin);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = vin.Id }, vin);
        }

        // PUT: api/vins/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Vin vin)
        {
            if (id != vin.Id)
            {
                return BadRequest();
            }

            var existing = await _context.Vins.FindAsync(id);

            if (existing == null)
            {
                return NotFound();
            }

            existing.Nom = vin.Nom;
            existing.Type = vin.Type;
            existing.Stock = vin.Stock;
            existing.SeuilStockBas = vin.SeuilStockBas;
            existing.Prix = vin.Prix;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/vins/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var vin = await _context.Vins.FindAsync(id);

            if (vin == null)
            {
                return NotFound();
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

using CavisteApp.Api.Data;
using CavisteApp.Api.Entities;
using CavisteApp.DTOs.Clients;
using CavisteApp.DTOs.Vins;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using CavisteApp.Api.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CavisteApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClientsController : ControllerBase
    {

        private readonly CavisteDbContext _context;

        public ClientsController(CavisteDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClientDto>>> GetAllClientDto()
        {
            var client = await _context.Clients
                .ToListAsync();

            return Ok(client);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ClientDto>> GetByIdClientDto(int id)
        {
            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null)
            {
                return NotFound();
            }

            return Ok(client);
        }

        [HttpPost]
        public async Task<ActionResult<ClientDto>> CreateClientDto(Client client)
        {
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetByIdClientDto), new { id = client.Id }, client);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = RolesConstants.Administrateur)] // Contrôle de rôle Identity
        public async Task<IActionResult> UpdateClientDto(int id, Client client
            )
        {
            if (id != client.Id)
            {
                return BadRequest();
            }

            var existing = await _context.Clients.FindAsync(id);

            if (existing == null)
            {
                return NotFound();
            }

            existing.Nom = client.Nom;
            existing.Prenom = client.Prenom;
            existing.NomRue = client.NomRue;
            existing.NumRue = client.NumRue;
            existing.Email = client.Email;
            existing.Telephone = client.Telephone;
            existing.CodePostal = client.CodePostal;
            existing.Ville = client.Ville;
            existing.DateCreation = client.DateCreation;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = RolesConstants.Administrateur)] // Contrôle de rôle Identity
        public async Task<IActionResult> DeleteClientDto(int id)
        {
            var client = await _context.Clients.FindAsync(id);

            if (client == null)
            {
                return NotFound();
            }

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static ClientDto ClientMapToDto(ClientDto client)
        {
            return new ClientDto
            {
                Id = client.Id,
                Nom = client.Nom,
                Prenom = client.Prenom,
                NomRue = client.NomRue,
                NumRue = client.NumRue,
                CodePostal = client.CodePostal,
                Ville = client.Ville,
                DateCreation = client.DateCreation,
                Email = client.Email,
                Telephone = client.Telephone
            };
        }
    }
}

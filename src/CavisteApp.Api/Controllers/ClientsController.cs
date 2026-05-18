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
        public async Task<ActionResult<IEnumerable<ClientDto>>> GetAll()
        {
            var clients = await _context.Clients
                .ToListAsync();

            return Ok(clients.Select(MapToDto));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ClientDto>> GetById(int id)
        {
            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null)
            {
                return NotFound($"Le client avec Id '{id}' n'existe pas.");
            }

            return Ok(MapToDto(client));
        }

        [HttpPost]
        public async Task<ActionResult<ClientDto>> Create([FromBody] CreerClientDto request)
        {
            var client = new Client
            {
                Nom = request.Nom,
                Prenom = request.Prenom,
                NomRue = request.NomRue,
                NumRue = request.NumRue,
                CodePostal = request.CodePostal,
                Ville = request.Ville,
                DateCreation = DateTime.UtcNow,
                Email = request.Email,
                Telephone = request.Telephone
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = client.Id }, MapToDto(client));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = RolesConstants.Administrateur)] // Contrôle de rôle Identity
        public async Task<IActionResult> Update(int id, [FromBody] UpdateClientDto request)
        {
            var client = await _context.Clients.FindAsync(id);

            if (client == null)
            {
                return NotFound($"Le client avec Id '{id}' n'existe pas.");
            }

            client.Nom = request.Nom;
            client.Prenom = request.Prenom;
            client.NomRue = request.NomRue;
            client.NumRue = request.NumRue;
            client.Email = request.Email;
            client.Telephone = request.Telephone;
            client.CodePostal = request.CodePostal;
            client.Ville = request.Ville;
            
            await _context.SaveChangesAsync();
            return Ok(MapToDto(client));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = RolesConstants.Administrateur)] // Contrôle de rôle Identity
        public async Task<IActionResult> Delete(int id)
        {
            var client = await _context.Clients.FindAsync(id);

            if (client == null)
            {
                return NotFound($"Le client avec Id '{id}' n'existe pas.");
            }

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static ClientDto MapToDto(Client client)
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

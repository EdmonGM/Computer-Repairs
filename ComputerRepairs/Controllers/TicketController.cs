using ComputerRepairs.Data;
using ComputerRepairs.DTOs.Ticket;
using ComputerRepairs.Mappers;
using ComputerRepairs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ComputerRepairs.Controllers
{
    [Route("api/ticket")]
    [ApiController]
    [Authorize]
    public class TicketController(AppDBContext context, UserManager<AppUser> userManager) : ControllerBase
    {
        private readonly AppDBContext _context = context;
        private readonly UserManager<AppUser> _userManager = userManager;

        // GET: api/<TicketController>
        [HttpGet]
        public async Task<IActionResult> GetAllTickets()
        {
            var tickets = await _context.Tickets.Select(t => t.ToTicketDto()).ToListAsync();
            return Ok(tickets);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetTicket([FromRoute] int id)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(x => x.Id == id);
            if (ticket == null)
            {
                return NotFound("No Ticket found with the given ID");
            }
            return Ok(ticket.ToTicketDto());
        }

        [HttpPost]
        public async Task<IActionResult> CreateTicket([FromBody] CreateTicketDto ticketDto)
        {
            var user = GetCurrentUser();

            if (user == null)
            {
                return Unauthorized();
            }
            var ticketModel = ticketDto.ToTicketFromCreateTicketDto(user.Id);
            await _context.AddAsync(ticketModel);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateTicket([FromRoute] int id, [FromBody] CreateTicketDto ticketDto)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(x => x.Id == id);
            if (ticket == null)
            {
                return NotFound("Ticket with id " + id + " not found!");
            }
            ticket.Title = ticketDto.Title;
            ticket.Description = ticketDto.Description;
            ticket.IsCompleted = ticketDto.IsCompleted;
            ticket.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(ticket);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteTicket([FromRoute] int id)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(x => x.Id == id);
            if (ticket == null)
            {
                return NotFound("Ticket with id " + id + " not found!");
            }
            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private AppUser GetCurrentUser()
        {
            ClaimsIdentity? identity = HttpContext.User.Identity as ClaimsIdentity;

            if (identity == null) return null;

            var claims = identity.Claims;

            return new AppUser
            {
                Id = claims.FirstOrDefault(c => c.Type == "id")?.Value,
                UserName = claims.FirstOrDefault(c => c.Type == "username")?.Value,
                Email = claims.FirstOrDefault(c => c.Type == "email")?.Value,
            };
        }
    }
}

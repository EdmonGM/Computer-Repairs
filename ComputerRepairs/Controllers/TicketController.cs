using ComputerRepairs.Data;
using ComputerRepairs.DTOs.Ticket;
using ComputerRepairs.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ComputerRepairs.Controllers
{
    [Route("api/tickets")]
    [ApiController]
    [Authorize]
    public class TicketController(AppDBContext context) : ControllerBase
    {
        private readonly AppDBContext _context = context;

        // GET: api/<TicketController>
        [HttpGet]
        [Authorize(Roles = "Admin")]
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
            string? userId = ((ClaimsIdentity)User.Identity!).FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var ticketModel = ticketDto.ToTicketFromCreateTicketDto(userId);
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
        [Authorize(Roles = "Admin")]
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
    }
}

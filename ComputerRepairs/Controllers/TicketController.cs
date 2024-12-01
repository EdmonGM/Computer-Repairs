using ComputerRepairs.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ComputerRepairs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController(AppDBContext context) : ControllerBase
    {
        private readonly AppDBContext _context = context;

        // GET: api/<TicketController>
        [HttpGet]
        public async Task<IActionResult> GetAllTickets()
        {
            var tickets = await _context.Tickets.ToListAsync();
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
            return Ok(ticket);
        }
        [HttpPost]
        public async Task<IActionResult> CreateTicket()
        {

        }
    }
}

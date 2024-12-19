using ComputerRepairs.Data;
using ComputerRepairs.DTOs.Account;
using ComputerRepairs.Mappers;
using ComputerRepairs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ComputerRepairs.Controllers
{
    [Route("api/app-user")]
    [ApiController]
    public class AppUserController(UserManager<AppUser> userManager, AppDBContext dBContext) : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly AppDBContext _dbContext = dBContext;


        [HttpPost("sign-up")]
        [Authorize]
        public async Task<IActionResult> CreateUser([FromBody] RegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid || registerDto.Password == null) return BadRequest("All fields are required");

                var appUser = new AppUser
                {
                    UserName = registerDto.Username,
                    Email = registerDto.Email,
                };
                var createdUser = await _userManager.CreateAsync(appUser, registerDto.Password);

                if (createdUser.Succeeded) return Ok(appUser.UserName + " user created!");
                else return BadRequest(createdUser.Errors);
            } catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("tickets")]
        [Authorize]
        public async Task<IActionResult> GetUserTickets()
        {
            string? userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }
            var tickets = await _dbContext.Tickets.Where(t => t.UserId == userId).ToListAsync();
            return Ok(tickets.Select(t => t.ToTicketDto()));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserById(string userId)
        {
            //var user = await _dbContext.Users.Include(t => t.Tickets).FirstOrDefaultAsync(u => u.Id ==  userId);
            var user = await _dbContext.Users.Include(t => t.Tickets).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound("No user found with the given ID");
            }
            return Ok(user.ToAppUserDto());
        }
        [HttpGet("current")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            string? userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var user = await _dbContext.Users.Include(t => t.Tickets).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound("No user found with the given ID");
            }
            return Ok(user.ToAppUserDto());
        }

        private string? GetCurrentUserId()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (identity == null) return null;
            
            var claims = identity.Claims;

            return claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        }

    }
}

using ComputerRepairs.Data;
using ComputerRepairs.DTOs.Account;
using ComputerRepairs.DTOs.AppUser;
using ComputerRepairs.Mappers;
using ComputerRepairs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
            var user = GetCurrentUser();
            if (user == null)
            {
                return Unauthorized();
            }
            var tickets = await _dbContext.Tickets.Where(t => t.UserId == user.Id).ToListAsync();
            return Ok(tickets);
        }

        [HttpGet]
        //[Authorize]
        public async Task<IActionResult> GetUser(string userId)
        {
            //var user = await _dbContext.Users.Include(t => t.Tickets).FirstOrDefaultAsync(u => u.Id ==  userId);
            var user = await _dbContext.Users.Include(t => t.Tickets).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound("No user found with the given ID");
            }
            return Ok(user.ToAppUserDto());
        }

        private AppUser? GetCurrentUser()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (identity == null) return null;

            var claims = identity.Claims;

#pragma warning disable CS8601 // Possible null reference assignment.
            return new AppUser
            {
                Id = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value,
                UserName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value,
                Email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
            };
#pragma warning restore CS8601 // Possible null reference assignment.
        }

    }
}

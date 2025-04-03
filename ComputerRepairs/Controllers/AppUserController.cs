using ComputerRepairs.Data;
using ComputerRepairs.DTOs.Account;
using ComputerRepairs.DTOs.AppUser;
using ComputerRepairs.Mappers;
using ComputerRepairs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Security.Claims;

namespace ComputerRepairs.Controllers
{
    [Route("api/app-users")]
    [ApiController]
    [Authorize]
    public class AppUserController(UserManager<AppUser> userManager, AppDBContext dBContext) : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly AppDBContext _dbContext = dBContext;

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {

            var users = await _dbContext.Users.ToListAsync();
            var usersDtos = new List<AppUserDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault();
                usersDtos.Add(user.ToAppUserDto(role ?? ""));
            }

            return Ok(usersDtos);
        }

        [HttpPost("sign-up")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] RegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("All fields are required");
                }

                var appUser = new AppUser
                {
                    UserName = registerDto.Username,
                    Email = registerDto.Email,
                };
                var createdUser = await _userManager.CreateAsync(appUser, registerDto.Password!);
                await _userManager.AddToRoleAsync(appUser, registerDto.Role);

                if (createdUser.Succeeded) return Ok(appUser.UserName + " user created!");
                else return BadRequest(createdUser.Errors);
            }
            catch (Exception ex)
            {
                Log.Error("Error while creating new user");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("tickets")]
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

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserById([FromRoute] string userId)
        {
            var user = await _dbContext.Users.Include(t => t.Tickets).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                Log.Warning("User #{id} not found", userId);
                return NotFound("No user found with the given ID");
            }
            var role = await _userManager.GetRolesAsync(user);
            return Ok(user.ToAppUserDto(role.FirstOrDefault() ?? ""));
        }
        [HttpPut("{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser([FromRoute] string userId,[FromBody] UpdateAppUserDto appUserDto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return BadRequest("No user found with the given Id");
            }
            user.UserName = appUserDto.UserName;
            user.Email = appUserDto.Email;
            user.Name = appUserDto.Name;
            user.Salary = appUserDto.Salary;
            
            if (!string.IsNullOrEmpty(appUserDto.Password))
            {
                await _userManager.RemovePasswordAsync(user);
                await _userManager.AddPasswordAsync(user, appUserDto.Password);
            }
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }
        [HttpGet("current")]
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
            var role = await _userManager.GetRolesAsync(user);
            return Ok(user.ToAppUserDto(role.FirstOrDefault() ?? ""));
        }
        [HttpPut("current/edit")]
        public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateCurrentAppUserDto appUserDto)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("Id is required");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return BadRequest("No user found with the given id");
            }
            user.Name = appUserDto.Name;
            user.Email = appUserDto.Email;

            // Checks if user wants to change the password 
            if (!string.IsNullOrEmpty(appUserDto.NewPassword))
            {
                if (string.IsNullOrEmpty(appUserDto.CurrentPassword))
                {
                    return BadRequest("Current Password is required");
                }

                var passwordChangeResult = await _userManager.ChangePasswordAsync(user, appUserDto.CurrentPassword, appUserDto.NewPassword);

                if (!passwordChangeResult.Succeeded)
                {
                    return BadRequest($"Error while changing password! {passwordChangeResult.Errors}");
                }

            }
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest($"Failed to update user {result.Errors}");
            }
            return Ok("User updated successfully");
        }

        [HttpDelete("{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser([FromRoute] string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("No user found with the given id");
            }
            var currentUserId = GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Forbid();
            }
            if(currentUserId == userId)
            {
                return BadRequest("You can't delete your own account");
            }
            var tickets = await _dbContext.Tickets.Where(t => t.UserId == userId).ToListAsync();
            foreach (var ticket in tickets)
            {
                _dbContext.Tickets.Remove(ticket);
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description));
            }

            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

        private string? GetCurrentUserId()
        {
            ClaimsIdentity? identity = User.Identity as ClaimsIdentity;

            if (identity == null) return null;

            var claims = identity.Claims;

            return claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        }

    }
}

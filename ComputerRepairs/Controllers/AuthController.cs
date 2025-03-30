using ComputerRepairs.Data;
using ComputerRepairs.DTOs.Account;
using ComputerRepairs.Filters;
using ComputerRepairs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.IdentityModel.Tokens.Jwt;

namespace ComputerRepairs.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController(UserManager<AppUser> userManager, TokenGenerator tokenGenerator, AppDBContext dbContext) : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly TokenGenerator _tokenGenerator = tokenGenerator;
        private readonly AppDBContext _dbContext = dbContext;

        private readonly CookieOptions accessCookieOptions = new()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.Now.AddMinutes(3)
        };

        [AllowAnonymous]
        [LogFieldValidation]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.Username!);
            if (user == null)
            {
                return NotFound("No user found with the given data");
            }

            var result = await _userManager.CheckPasswordAsync(user, loginDto.Password!);
            if (!result)
            {
                return BadRequest("Incorrect Password");
            }

            var accessToken = _tokenGenerator.GenerateAccessToken(user);
            Response.Cookies.Append("access", accessToken, accessCookieOptions);

            var refreshToken = _tokenGenerator.GenerateRefreshToken(user);
            var refreshCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.Now.AddDays(7),
            };
            Response.Cookies.Append("refresh", refreshToken, refreshCookieOptions);

            return Ok();
        }
        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies.FirstOrDefault(t => t.Key == "refresh").Value;
            if(refreshToken == null)
            {
                Log.Information("User refresh token is not found");
                return BadRequest("No refresh token found");
            }
            var principal = _tokenGenerator.GetPrincipalFromRefreshCookie(refreshToken);
            var userId = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
            var username = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.UniqueName)?.Value;

            Log.Information("{username} is trying to refresh their token", username);
            if (userId == null)
            {
                Log.Warning("UserId not found in the provided refresh token");
                return NotFound("UserId not found");
            }

            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                Log.Warning("No user found with provided id");
                return NotFound("User not found");
            }

            var newAccessToken = _tokenGenerator.GenerateAccessToken(user);
            Response.Cookies.Append("access", newAccessToken, accessCookieOptions);
            Log.Information("{username} successfully refreshed their token", username);

            return Ok();
        }
    }
}

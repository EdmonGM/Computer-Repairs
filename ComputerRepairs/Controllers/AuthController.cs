using ComputerRepairs.Data;
using ComputerRepairs.DTOs.Account;
using ComputerRepairs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ComputerRepairs.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController(IConfiguration config, UserManager<AppUser> userManager, TokenGenerator tokenGenerator, AppDBContext dbContext) : ControllerBase
    {
        private readonly IConfiguration _config = config;
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
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("All fileds are required");
            }

            var user = await _userManager.FindByNameAsync(loginDto.Username);
            if (user == null)
            {
                return NotFound("No user found with the given data");
            }

            var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);
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
                return Unauthorized("No refresh token found");
            }
            var principal = _tokenGenerator.GetPrincipalFromRefreshCookie(refreshToken);
            foreach (var item in principal.Claims.Select(c => c))
            {
                await Console.Out.WriteLineAsync(item.Type);
            }
            var userId = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
            if (userId == null)
            {
                return BadRequest("UserId not found");
            }
            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                return NotFound("User not found");
            }
            var newAccessToken = _tokenGenerator.GenerateAccessToken(user);
            Response.Cookies.Append("access", newAccessToken, accessCookieOptions);

            return Ok();
        }
    }
}

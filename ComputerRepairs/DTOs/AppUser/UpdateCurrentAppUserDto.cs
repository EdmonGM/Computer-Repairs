using ComputerRepairs.DTOs.Ticket;
using System.ComponentModel.DataAnnotations;

namespace ComputerRepairs.DTOs.AppUser
{
    public class UpdateCurrentAppUserDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string NewPassword{ get; set; } = string.Empty;
        public string CurrentPassword { get; set; } = string.Empty;
    }
}

using ComputerRepairs.DTOs.Ticket;
using System.ComponentModel.DataAnnotations;

namespace ComputerRepairs.DTOs.AppUser
{
    public class UpdateCurrentAppUserDto
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [MinLength(3)]
        public string Name { get; set; } = string.Empty;
        public string NewPassword{ get; set; } = string.Empty;
        public string CurrentPassword { get; set; } = string.Empty;
    }
}

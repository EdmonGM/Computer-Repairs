using System.ComponentModel.DataAnnotations;

namespace ComputerRepairs.DTOs.AppUser
{
    public class UpdateAppUserDto
    {
        public string UserName { get; set; }
        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Salary { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComputerRepairs.DTOs.Account
{
    public class RegisterDto
    {
        [Required]
        [MinLength(4)]
        [MaxLength(18)]
        public string Username { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string Name { get; set; }
        [Required]
        [MinLength(8)]
        [MaxLength(32)]
        public string Password { get; set; }
        public string Role { get; set; } = "Employee";
        [Column(TypeName = "decimal(6,2)")]
        [Range(0.00, 999999.99)]
        public decimal Salary { get; set; } = decimal.Zero;
    }
}

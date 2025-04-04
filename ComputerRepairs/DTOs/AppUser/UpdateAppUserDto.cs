using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComputerRepairs.DTOs.AppUser
{
    public class UpdateAppUserDto
    {
        [Required]
        [MinLength(4)]
        [MaxLength(18)]
        public string UserName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        [MinLength(8)]
        [MaxLength(32)]
        public string Password { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        [Column(TypeName = "decimal(6,2)")]
        [Range(0.00, 999999.99)]
        public decimal Salary { get; set; }
    }
}

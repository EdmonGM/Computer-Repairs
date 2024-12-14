using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComputerRepairs.Models
{
    public class AppUser : IdentityUser
    {
        [PersonalData]
        public string? Name { get; set; }
        [Column(TypeName = "decimal(6,2)")]
        public decimal Salary { get; set; } = decimal.Zero;
        public List<Ticket> Tickets { get; set; } = [];
    }
}

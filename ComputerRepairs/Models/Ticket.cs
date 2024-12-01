using System.ComponentModel.DataAnnotations;

namespace ComputerRepairs.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        [Required]
        [MinLength(5)]
        [MaxLength(50)]
        public string Title { get; set; } = string.Empty;
        [MaxLength(300)]
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set;}
        public bool IsCompleted { get; set; } = false;
    }
}
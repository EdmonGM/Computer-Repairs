using System.ComponentModel.DataAnnotations;

namespace ComputerRepairs.DTOs.Ticket
{
    public class CreateTicketDto
    {
        [Required]
        [MinLength(5, ErrorMessage = "Title should NOT be less than 5 letters")]
        [MaxLength(50, ErrorMessage = "Title should NOT be more than 50 letters")]
        public string Title { get; set; } = string.Empty;
        [MaxLength(300, ErrorMessage = "Description should NOT be more than 300 letters")]
        public string Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; } = false;
    }
}

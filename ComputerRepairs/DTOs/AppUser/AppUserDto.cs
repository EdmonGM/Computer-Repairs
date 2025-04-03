using ComputerRepairs.DTOs.Ticket;

namespace ComputerRepairs.DTOs.AppUser
{
    public class AppUserDto
    {
        public required string Id { get; set; }
        public required string UserName { get; set;}
        public required string Email { get; set;}
        public string? Name { get; set;}
        public decimal Salary { get; set;}
        public string? Role { get; set;}
        public List<TicketDto>? Tickets { get; set;}
    }
}

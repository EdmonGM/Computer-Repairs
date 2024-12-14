using ComputerRepairs.DTOs.Ticket;

namespace ComputerRepairs.DTOs.AppUser
{
    public class AppUserDto
    {
        public string Id { get; set; }
        public string UserName { get; set;}
        public string Email { get; set;}
        public string Name { get; set;}
        public decimal Salary { get; set;}
        public List<TicketDto> Tickets { get; set;}
    }
}

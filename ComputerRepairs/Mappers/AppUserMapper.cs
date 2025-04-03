using ComputerRepairs.DTOs.AppUser;
using ComputerRepairs.Models;

namespace ComputerRepairs.Mappers
{
    public static class AppUserMapper
    {
        public static AppUserDto ToAppUserDto(this AppUser user, string role)
        {
            return new AppUserDto
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                Name = user.Name,
                Salary = user.Salary,
                Role = role,
                Tickets = user.Tickets.Select(t => t.ToTicketDto()).ToList(),
            };
        }
    }
}

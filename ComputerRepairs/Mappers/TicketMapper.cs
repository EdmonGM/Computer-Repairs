using ComputerRepairs.DTOs.Ticket;
using ComputerRepairs.Models;

namespace ComputerRepairs.Mappers
{
    public static class TicketMapper
    {
        public static TicketDto ToTicketDto(this Ticket ticketModel)
        {
            return new TicketDto
            {
                Id = ticketModel.Id,
                UserId = ticketModel.UserId,
                Title = ticketModel.Title,
                Description = ticketModel.Description,
                CreatedDate = ticketModel.CreatedDate,
                UpdatedDate = ticketModel.UpdatedDate,
                IsCompleted = ticketModel.IsCompleted,
            };
        }
        public static Ticket ToTicketFromCreateTicketDto(this CreateTicketDto ticketDto, string userId)
        {
            return new Ticket
            {
                Title = ticketDto.Title,
                Description = ticketDto.Description,
                IsCompleted = ticketDto.IsCompleted,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                UserId = userId,
            };
        }
    }
}

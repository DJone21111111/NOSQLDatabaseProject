using IncidentManagementsSystemNOSQL.Models;
namespace IncidentManagementsSystemNOSQL.Service
{
    public interface ITicketService
    {
        Task<List<Ticket>> GetAllTickets();
        Task<Ticket?> GetTicketById(string id);
        Task<List<Ticket>> GetTicketsByUserId(string userId);
        Task<List<Ticket>> GetTicketsByStatus(Enums.TicketStatus status);
        Task<List<Ticket>> GetTicketsByDateRange(DateTime startDate, DateTime endDate);
        Task AddTicket(Ticket ticket);
        Task UpdateTicket(string id, Ticket updatedTicket);
        Task DeleteTicket(string id);
    }
}

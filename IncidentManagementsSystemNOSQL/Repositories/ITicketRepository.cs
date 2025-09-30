using IncidentManagementsSystemNOSQL.Models;

namespace IncidentManagementsSystemNOSQL.Repositories
{
    public interface ITicketRepository
    {
        Task<Ticket?> GetById(string id);
        Task<List<Ticket>> GetByUserId(string userId);
        Task<List<Ticket>> GetAll();
        Task<List<Ticket>> GetByStatus(Enums.TicketStatus status);

        // Mutations
        Task AddTicket(Ticket ticket);
        Task UpdateTicket(Ticket ticket);
        Task DeleteById(string id);

        // Dashboard/aggregations
        Task<Dictionary<Enums.TicketStatus, int>> GetTicketCountsByStatus();
        Task<Dictionary<string, int>> GetTicketCountsByDepartment();

        // Database preparation
        Task EnsureIndexesAsync(CancellationToken ct = default);
    }
}

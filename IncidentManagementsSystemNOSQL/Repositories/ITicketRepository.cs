using IncidentManagementsSystemNOSQL.Models;

namespace IncidentManagementsSystemNOSQL.Repositories
{
    public interface ITicketRepository
    {
        // READ Operations
        Ticket? GetById(string id);
        List<Ticket> GetByUserId(string userId);
        List<Ticket> GetAll();
        List<Ticket> GetByPriority(Enums.PriorityLevel priority);
        List<Ticket> GetByStatus(Enums.TicketStatus status);

        // Might need to filter by time range in the future. We are not using it for now.
        List<Ticket> GetByDateRange(DateTime startDate, DateTime endDate);

        void AddTicket(Ticket ticket);
        void UpdateTicket(string id, Ticket updated);
        void DeleteById(string id);

        // AGGREGATION Operations
        Dictionary<Enums.TicketStatus, int> GetTicketCountsByStatus();
        Dictionary<string, int> GetTicketCountsByDepartment();

        // SETUP Operation
        void EnsureIndexes();
    }
}

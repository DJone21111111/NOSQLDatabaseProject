using IncidentManagementsSystemNOSQL.Models;
namespace IncidentManagementsSystemNOSQL.Service
{
    public interface ITicketService
    {
        // READ Operations
        List<Ticket> GetAll();
        Ticket? GetById(string id);
        List<Ticket> GetByUserId(string userId);
        List<Ticket> GetByStatus(string status);

        List<Ticket> GetByDateRange(DateTime startDate, DateTime endDate);

        // MUTATION Operations
        void AddTicket(Ticket ticket);
        void UpdateTicket(string id, Ticket updatedTicket);
        void DeleteTicket(string id);

        // AGGREGATION Operations
        Dictionary<string, int> GetTicketCountsByStatus();
        Dictionary<string, int> GetTicketCountsByDepartment();
        Dictionary<string, int> GetTicketCountsByStatusForEmployee(string employeeId);

        string GetNextTicketId();
    }
}

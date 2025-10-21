using IncidentManagementsSystemNOSQL.Models;

namespace IncidentManagementsSystemNOSQL.Repositories
{
    public interface ITicketRepository
    {
        Ticket? GetById(string id);
        List<Ticket> GetByUserId(string userId);
        List<Ticket> GetAll();
        List<Ticket> GetByStatus(Enums.TicketStatus status);
        List<Ticket> GetByDateRange(DateTime startDate, DateTime endDate);
        void AddTicket(Ticket ticket);
        void UpdateTicket(string id, Ticket updated);
        void DeleteById(string id);
        Dictionary<Enums.TicketStatus, int> GetTicketCountsByStatus();
        Dictionary<Enums.DepartmentType, int> GetTicketCountsByDepartment();
        Dictionary<Enums.TicketStatus, int> GetTicketCountsByStatusForEmployee(string employeeId);
        string GetNextTicketId();
        int GetNextServiceDeskAgentIndex(int agentCount);
        void SetAssignedAgent(string ticketId, CommentAuthorEmbedded agent);
        void EnsureIndexes();
    }
}

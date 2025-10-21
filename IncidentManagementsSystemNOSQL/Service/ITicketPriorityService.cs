using IncidentManagementsSystemNOSQL.Models;
using MongoDB.Driver;


namespace IncidentManagementsSystemNOSQL.Service
{
    namespace IncidentManagementsSystemNOSQL.Service
    {
        public interface ITicketPriorityService
        {
            // Returns a filter for a specific priority
            FilterDefinition<Ticket> BuildPriorityFilter(Enums.TicketPriority priority);

            // Helper: parse "critical"/"Critical"/"HIGH" → TicketPriority
            bool TryParsePriority(string? input, out Enums.TicketPriority priority);

        }
    }
}


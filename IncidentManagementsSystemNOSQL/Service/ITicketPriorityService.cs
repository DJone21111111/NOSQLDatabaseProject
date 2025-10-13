using IncidentManagementsSystemNOSQL.Models;
using MongoDB.Driver;


namespace IncidentManagementsSystemNOSQL.Service
{
    public interface ITicketPriorityService
    {
        SortDefinition<Ticket> BuildSort(string sortKey);
        bool IsSupported(string sortKey);
    }

    public class TicketSortService : ITicketPriorityService
    {
        public bool IsSupported(string sortKey) => sortKey is "priority" or "recent" or "oldest";

        public SortDefinition<Ticket> BuildSort(string sortKey)
        {
            var sort = Builders<Ticket>.Sort;

            return sortKey switch
            {
                // Primary: Priority (enum order Critical→Low), Secondary: CreatedAt desc (newest first)
                "priority" => sort.Ascending(t => t.Priority)
                                  .Descending(t => t.DateCreated),

                // Newest first
                "recent" => sort.Descending(t => t.DateCreated),

                // Oldest first
                "oldest" => sort.Ascending(t => t.DateCreated),

                // Safe default: priority sort
                _ => sort.Ascending(t => t.Priority)
                                  .Descending(t => t.DateCreated),
            };
        }
    }
}


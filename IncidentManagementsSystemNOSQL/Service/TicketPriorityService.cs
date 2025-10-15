using IncidentManagementsSystemNOSQL.Models;
using IncidentManagementsSystemNOSQL.Repositories;
using IncidentManagementsSystemNOSQL.Service.IncidentManagementsSystemNOSQL.Service;
using MongoDB.Driver;
using System.Globalization;

namespace IncidentManagementsSystemNOSQL.Service
{
    public class TicketPriorityService : ITicketPriorityService
    {
        private readonly ITicketRepository _ticketRepository;

        public TicketPriorityService(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        public FilterDefinition<Ticket> BuildPriorityFilter(Enums.PriorityLevel priority)
        {
            var filter = Builders<Ticket>.Filter;
            return filter.Eq("priority", priority.ToString().ToLower());
        }


        public bool TryParsePriority(string? input, out Enums.PriorityLevel priority)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                priority = default;
                return false;
            }

            // Accepts case-insensitive names: "critical", "High", etc.
            if (Enum.TryParse<Enums.PriorityLevel>(input, ignoreCase: true, out var parsed))
            {
                priority = parsed;
                return true;
            }

            // Accept numeric values if your enum is 0..N (optional)
            if (int.TryParse(input, out var num) &&
                Enum.IsDefined(typeof(Enums.PriorityLevel), num))
            {
                priority = (Enums.PriorityLevel)num;
                return true;
            }

            priority = default;
            return false;
        }
    }
}
using IncidentManagementsSystemNOSQL.Repositories;

namespace IncidentManagementsSystemNOSQL.Service
{
    public class TicketPriorityService : ITicketPriorityService
    {
        private readonly ITicketRepository _ticketRepository;

        public TicketService(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }
    }

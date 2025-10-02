using IncidentManagementsSystemNOSQL.Models;
using IncidentManagementsSystemNOSQL.Repositories;
namespace IncidentManagementsSystemNOSQL.Service
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;

        public TicketService(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }


        public List<Ticket> GetAll()
        {
            try
            {
                return _ticketRepository.GetAll();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while retrieving all tickets.", ex);
            }
        }

        public Ticket? GetById(string id)
        {
            try
            {
                return _ticketRepository.GetById(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while retrieving ticket with ID '{id}'.", ex);
            }
        }

        public List<Ticket> GetByUserId(string userId)
        {
            try
            {
                return _ticketRepository.GetByUserId(userId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while retrieving tickets for userId '{userId}'.", ex);
            }
        }

        public List<Ticket> GetByStatus(Enums.TicketStatus status)
        {
            try
            {
                return _ticketRepository.GetByStatus(status);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while retrieving tickets with status '{status}'.", ex);
            }
        }


        public List<Ticket> GetByDateRange(DateTime startDate, DateTime endDate)
        {
            try
            {
                return _ticketRepository.GetByDateRange(startDate, endDate);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while retrieving tickets by date range.", ex);
            }
        }


        public void AddTicket(Ticket ticket)
        {
            try
            {
                ticket.DateCreated = DateTime.UtcNow;
                ticket.Status = Enums.TicketStatus.open;

                _ticketRepository.AddTicket(ticket);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while adding ticket '{ticket.Title}'.", ex);
            }
        }

        public void UpdateTicket(string id, Ticket updatedTicket)
        {
            updatedTicket.Id = id;

            try
            {
                if (updatedTicket.Status == Enums.TicketStatus.closed_resolved)
                {
                    updatedTicket.DateClosed = DateTime.UtcNow;
                }

                _ticketRepository.UpdateTicket(id,updatedTicket);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while updating ticket '{updatedTicket.Id}'.", ex);
            }
        }

        public void DeleteTicket(string id)
        {
            try
            {
                _ticketRepository.DeleteById(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while deleting ticket with ID '{id}'.", ex);
            }
        }


        public Dictionary<Enums.TicketStatus, int> GetTicketCountsByStatus()
        {
            try
            {
                return _ticketRepository.GetTicketCountsByStatus();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while retrieving ticket counts by status.", ex);
            }
        }

        public Dictionary<string, int> GetTicketCountsByDepartment()
        {
            try
            {
                return _ticketRepository.GetTicketCountsByDepartment();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while retrieving ticket counts by department.", ex);
            }
        }
    }
}

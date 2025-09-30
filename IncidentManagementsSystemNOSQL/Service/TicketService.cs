using IncidentManagementsSystemNOSQL.Models;
using IncidentManagementsSystemNOSQL.Repositories;

namespace IncidentManagementsSystemNOSQL.Service
{
    public class TicketService
    {
        private readonly ITicketRepository _ticketRepository;

        public TicketService(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        public async Task<List<Ticket>> GetAllTickets()
        {
            try
            {
                return await _ticketRepository.GetAll();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while retrieving all tickets.", ex);
            }
        }

        public async Task<Ticket?> GetTicketById(string id)
        {
            try
            {
                return await _ticketRepository.GetById(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while retrieving ticket with ID '{id}'.", ex);
            }
        }

        public async Task<List<Ticket>> GetTicketsByUserId(string userId)
        {
            try
            {
                return await _ticketRepository.GetByUserId(userId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while retrieving tickets for userId '{userId}'.", ex);
            }
        }

        public async Task<List<Ticket>> GetTicketsByStatus(Enums.TicketStatus status)
        {
            try
            {
                return await _ticketRepository.GetByStatus(status);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while retrieving tickets with status '{status}'.", ex);
            }
        }

        public async Task AddTicket(Ticket ticket)
        {
            try
            {
                ticket.DateCreated = DateTime.UtcNow;
                ticket.Status = Enums.TicketStatus.open;
                await _ticketRepository.AddTicket(ticket);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while adding ticket '{ticket.Title}'.", ex);
            }
        }

        public async Task UpdateTicket(Ticket ticket)
        {
            try
            {
                if (ticket.Status == Enums.TicketStatus.closed_resolved)
                {
                    ticket.DateClosed = DateTime.UtcNow;
                }

                await _ticketRepository.UpdateTicket(ticket);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while updating ticket '{ticket.Id}'.", ex);
            }
        }

        public async Task DeleteTicket(string id)
        {
            try
            {
                await _ticketRepository.DeleteById(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while deleting ticket with ID '{id}'.", ex);
            }
        }

        public async Task<Dictionary<Enums.TicketStatus, int>> GetTicketCountsByStatusAsync()
        {
            try
            {
                return await _ticketRepository.GetTicketCountsByStatus();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while retrieving ticket counts by status.", ex);
            }
        }

        public async Task<Dictionary<string, int>> GetTicketCountsByDepartment()
        {
            try
            {
                return await _ticketRepository.GetTicketCountsByDepartment();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while retrieving ticket counts by department.", ex);
            }
        }
    }
}

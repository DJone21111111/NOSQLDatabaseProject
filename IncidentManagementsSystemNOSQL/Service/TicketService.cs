using IncidentManagementsSystemNOSQL.Models;
using IncidentManagementsSystemNOSQL.Repositories;

namespace IncidentManagementsSystemNOSQL.Service
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IUserService _userService;

        public TicketService(ITicketRepository ticketRepository, IUserService userService)
        {
            _ticketRepository = ticketRepository;
            _userService = userService;
        }

        public List<Ticket> GetAll()
        {
            try
            {
                var tickets = _ticketRepository.GetAll();
                BackfillAssignmentsIfNeeded(tickets);
                return tickets;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while retrieving all tickets.", ex);
            }
        }

        public List<Ticket> GetAllSortedByPriority(bool descending = true)
        {
            try
            {
                var tickets = _ticketRepository.GetAll();
                BackfillAssignmentsIfNeeded(tickets);

                // Define priority order: High > Medium > Low
                var priorityOrder = new Dictionary<Enums.TicketPriority, int>
                {
                    { Enums.TicketPriority.High, 3 },
                    { Enums.TicketPriority.Medium, 2 },
                    { Enums.TicketPriority.Low, 1 }
                };

                if (descending)
                {
                    // High to Low priority
                    return tickets.OrderByDescending(t => t.Priority.HasValue ? priorityOrder[t.Priority.Value] : 0)
                                  .ThenBy(t => t.DateCreated)
                                  .ToList();
                }
                else
                {
                    // Low to High priority
                    return tickets.OrderBy(t => t.Priority.HasValue ? priorityOrder[t.Priority.Value] : 0)
                                  .ThenBy(t => t.DateCreated)
                                  .ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error while retrieving and sorting tickets by priority.", ex);
            }
        }

        public List<Ticket> GetByPriority(Enums.TicketPriority priority)
        {
            try
            {
                var tickets = _ticketRepository.GetAll();
                BackfillAssignmentsIfNeeded(tickets);

                // Filter tickets by the specified priority
                return tickets.Where(t => t.Priority.HasValue && t.Priority.Value == priority)
                              .OrderBy(t => t.DateCreated)
                              .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while retrieving tickets with priority '{priority}'.", ex);
            }
        }

        public Ticket? GetById(string id)
        {
            try
            {
                var ticket = _ticketRepository.GetById(id);
                if (ticket != null)
                {
                    BackfillAssignmentsIfNeeded(new List<Ticket> { ticket });
                }

                return ticket;
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
                var tickets = _ticketRepository.GetByUserId(userId);
                BackfillAssignmentsIfNeeded(tickets);
                return tickets;
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
                var tickets = _ticketRepository.GetByStatus(status);
                BackfillAssignmentsIfNeeded(tickets);
                return tickets;
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
                var tickets = _ticketRepository.GetByDateRange(startDate, endDate);
                BackfillAssignmentsIfNeeded(tickets);
                return tickets;
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

                if (string.IsNullOrWhiteSpace(ticket.TicketId))
                {
                    ticket.TicketId = GetNextTicketId();
                }

                EnsureAssignedAgent(ticket);

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
                if (updatedTicket.Status == Enums.TicketStatus.closed_resolved || updatedTicket.Status == Enums.TicketStatus.closed_no_resolve)
                {
                    updatedTicket.DateClosed = DateTime.UtcNow;
                }
                else if (updatedTicket.Status == Enums.TicketStatus.open || updatedTicket.Status == Enums.TicketStatus.in_progress)
                {
                    updatedTicket.DateClosed = null;
                }

                EnsureAssignedAgent(updatedTicket);

                _ticketRepository.UpdateTicket(id, updatedTicket);
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

        public Dictionary<Enums.DepartmentType, int> GetTicketCountsByDepartment()
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

        public Dictionary<Enums.TicketStatus, int> GetTicketCountsByStatusForEmployee(string employeeId)
        {
            try
            {
                return _ticketRepository.GetTicketCountsByStatusForEmployee(employeeId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while retrieving ticket counts for employee {employeeId}.", ex);
            }
        }

        public string GetNextTicketId()
        {
            try
            {
                return _ticketRepository.GetNextTicketId();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while generating the next ticket ID.", ex);
            }
        }

        private void EnsureAssignedAgent(Ticket ticket)
        {
            if (ticket.AssignedTo != null)
            {
                return;
            }

            var agents = _userService.GetServiceDeskAgents();
            if (agents == null || agents.Count == 0)
            {
                throw new InvalidOperationException("No service desk agents are available to handle tickets.");
            }

            var agent = SelectNextServiceDeskAgent(agents);
            ticket.AssignedTo = agent;
        }

        private CommentAuthorEmbedded SelectNextServiceDeskAgent(List<User> agents)
        {
            if (agents.Count == 1)
            {
                return MapAgent(agents[0]);
            }

            var index = _ticketRepository.GetNextServiceDeskAgentIndex(agents.Count);
            var agent = agents[index];
            return MapAgent(agent);
        }

        private static CommentAuthorEmbedded MapAgent(User agent)
        {
            return new CommentAuthorEmbedded
            {
                EmployeeId = agent.EmployeeId,
                Name = agent.Name,
                Email = agent.Email,
                Role = agent.Role.ToString()
            };
        }

        private void BackfillAssignmentsIfNeeded(List<Ticket> tickets)
        {
            if (tickets == null || tickets.Count == 0)
            {
                return;
            }

            var missingAssignments = tickets.Where(t => t.AssignedTo == null).ToList();
            if (missingAssignments.Count == 0)
            {
                return;
            }

            var agents = _userService.GetServiceDeskAgents();
            if (agents == null || agents.Count == 0)
            {
                return;
            }

            foreach (var ticket in missingAssignments)
            {
                var agent = SelectNextServiceDeskAgent(agents);
                ticket.AssignedTo = agent;
                if (!string.IsNullOrWhiteSpace(ticket.Id))
                {
                    _ticketRepository.SetAssignedAgent(ticket.Id, agent);
                }
            }
        }
    }
}

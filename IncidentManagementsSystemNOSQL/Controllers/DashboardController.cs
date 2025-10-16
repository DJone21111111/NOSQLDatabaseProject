using IncidentManagementsSystemNOSQL.Models;
using IncidentManagementsSystemNOSQL.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IncidentManagementsSystemNOSQL.Controllers
{
    [Authorize(Roles = nameof(Enums.UserRole.service_desk))]
    public class DashboardController : Controller
    {
        private readonly ITicketService _ticketService;
        private readonly IUserService _userService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ITicketService ticketService, IUserService userService, ILogger<DashboardController> logger)
        {
            _ticketService = ticketService;
            _userService = userService;
            _logger = logger;
        }

        public IActionResult Index(string? agentId)
        {
            try
            {
                var agent = ResolveServiceDeskAgent(agentId);
                var allTickets = _ticketService.GetAll();
                var myTickets = new List<Ticket>();

                if (agent != null)
                {
                    myTickets = allTickets
                        .Where(t => t.AssignedTo != null &&
                                    string.Equals(t.AssignedTo.EmployeeId, agent.EmployeeId, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    ViewBag.AgentName = agent.Name;
                    ViewBag.AgentId = agent.EmployeeId;
                    ViewBag.AgentEmail = agent.Email;
                    ViewBag.HasAgentContext = true;
                }
                else
                {
                    TempData["ErrorMessage"] = "We could not resolve a service desk account to personalise assignments. Showing all tickets.";
                    ViewBag.HasAgentContext = false;
                }

                ViewBag.AgentTicketCount = myTickets.Count;

                // ENUM-KEYED DICTIONARIES
                Dictionary<Enums.TicketStatus, int> statusCounts = _ticketService.GetTicketCountsByStatus();
                Dictionary<Enums.DepartmentType, int> departmentCounts = _ticketService.GetTicketCountsByDepartment();

                ViewBag.StatusCounts = statusCounts;
                ViewBag.DepartmentCounts = departmentCounts;

                int totalTickets = allTickets.Count;
                ViewBag.TotalTickets = totalTickets;

                var statusPercentages = new Dictionary<Enums.TicketStatus, double>();
                if (totalTickets > 0)
                {
                    foreach (var kv in statusCounts)
                    {
                        statusPercentages[kv.Key] = Math.Round((double)kv.Value / totalTickets * 100, 1);
                    }
                }

                ViewBag.StatusPercentages = statusPercentages;

                return View(allTickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading service desk dashboard");
                TempData["ErrorMessage"] = "An error occurred while loading the ticket dashboard.";

                ViewBag.StatusCounts = new Dictionary<Enums.TicketStatus, int>();
                ViewBag.StatusPercentages = new Dictionary<Enums.TicketStatus, double>();
                ViewBag.DepartmentCounts = new Dictionary<Enums.DepartmentType, int>();
                ViewBag.TotalTickets = 0;
                ViewBag.HasAgentContext = false;
                ViewBag.AgentTicketCount = 0;

                return View(new List<Ticket>());
            }
        }

        private User? ResolveServiceDeskAgent(string? agentId)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(agentId))
                {
                    var agentById = _userService.GetUserByEmployeeId(agentId);
                    if (agentById != null && agentById.Role == Enums.UserRole.service_desk)
                    {
                        return agentById;
                    }
                }

                var agents = _userService.GetServiceDeskAgents();
                if (agents == null || agents.Count == 0) return null;

                const string defaultAgentName = "Zoe Garcia";
                var agentByName = agents.FirstOrDefault(a =>
                    string.Equals(a.Name, defaultAgentName, StringComparison.OrdinalIgnoreCase));
                return agentByName ?? agents.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resolve service desk agent for {AgentId}", agentId);
                return null;
            }
        }
    }
}

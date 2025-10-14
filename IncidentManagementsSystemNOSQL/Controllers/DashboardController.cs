using System;
using System.Collections.Generic;
using System.Linq;
using IncidentManagementsSystemNOSQL.Models;
using IncidentManagementsSystemNOSQL.Service;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Authorization;

namespace IncidentManagementsSystemNOSQL.Controllers
{
     //for later
   // [Authorize(Roles = "service_desk")]
 
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
                User? agent = ResolveServiceDeskAgent(agentId);
                List<Ticket> allTickets = _ticketService.GetAll();
                List<Ticket> myTickets = new List<Ticket>();

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

                Dictionary<string, int> statusCounts = _ticketService.GetTicketCountsByStatus();
                Dictionary<string, int> departmentCounts = _ticketService.GetTicketCountsByDepartment();

                ViewBag.StatusCounts = statusCounts;
                ViewBag.DepartmentCounts = departmentCounts;

                int totalTickets = allTickets.Count;
                ViewBag.TotalTickets = totalTickets;

                Dictionary<string, double> statusPercentages = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
                if (totalTickets > 0)
                {
                    foreach (string statusKey in statusCounts.Keys)
                    {
                        int count = statusCounts[statusKey];
                        statusPercentages[statusKey] = Math.Round((double)count / totalTickets * 100, 1);
                    }
                }

                ViewBag.StatusPercentages = statusPercentages;

                return View(allTickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading service desk dashboard");
                TempData["ErrorMessage"] = "An error occurred while loading the ticket dashboard.";
                ViewBag.StatusCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                ViewBag.StatusPercentages = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
                ViewBag.DepartmentCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
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
                    User? agentById = _userService.GetUserByEmployeeId(agentId);
                    if (agentById != null &&
                        string.Equals(agentById.Role, "service_desk", StringComparison.OrdinalIgnoreCase))
                    {
                        return agentById;
                    }
                }

                List<User> agents = _userService.GetServiceDeskAgents();
                if (agents == null || agents.Count == 0)
                {
                    return null;
                }

                const string defaultAgentName = "Zoe Garcia";
                User? agentByName = agents.FirstOrDefault(a => string.Equals(a.Name, defaultAgentName, StringComparison.OrdinalIgnoreCase));
                if (agentByName != null)
                {
                    return agentByName;
                }

                return agents.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resolve service desk agent for {AgentId}", agentId);
                return null;
            }
        }
    }
}
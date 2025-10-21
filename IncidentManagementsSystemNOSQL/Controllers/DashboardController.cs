using System.Security.Claims;
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

        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                ViewBag.HideUsersNav = false;
                ViewBag.EmployeeMode = false;

                var empId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var agent = !string.IsNullOrWhiteSpace(empId) ? _userService.GetUserByEmployeeId(empId) : null;
                var isAgent = agent != null && agent.IsActive && agent.Role == Enums.UserRole.service_desk;

                if (isAgent)
                {
                    ViewBag.HasAgentContext = true;
                    ViewBag.AgentId = agent.EmployeeId;
                    ViewBag.AgentName = agent.Name;
                    ViewBag.AgentEmail = agent.Email;
                }
                else
                {
                    ViewBag.HasAgentContext = false;
                    ViewBag.AgentId = "";
                    ViewBag.AgentName = "";
                    ViewBag.AgentEmail = "";
                    TempData["ErrorMessage"] = "We could not resolve a service desk account to personalise assignments. Showing all tickets.";
                }

                var tickets = _ticketService.GetAll();

                var byStatus = _ticketService.GetTicketCountsByStatus();
                var total = tickets.Count;

                var percentages = new Dictionary<Enums.TicketStatus, double>();
                foreach (var kv in byStatus)
                {
                    var p = total > 0 ? (double)kv.Value / total * 100.0 : 0.0;
                    percentages[kv.Key] = Math.Round(p, 1);
                }

                var deptCounts = _ticketService.GetTicketCountsByDepartment();

                ViewBag.TotalTickets = total;
                ViewBag.StatusCounts = byStatus;
                ViewBag.StatusPercentages = percentages;
                ViewBag.DepartmentCounts = deptCounts;

                if (isAgent)
                {
                    var myCount = tickets.Count(t => t.AssignedTo != null &&
                                                     string.Equals(t.AssignedTo.EmployeeId, agent.EmployeeId, StringComparison.OrdinalIgnoreCase));
                    ViewBag.AgentTicketCount = myCount;
                }
                else
                {
                    ViewBag.AgentTicketCount = 0;
                }

                return View(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading service desk dashboard");
                TempData["ErrorMessage"] = "An unexpected error occurred while loading the dashboard.";
                return View(new List<Ticket>());
            }
        }
    }
}

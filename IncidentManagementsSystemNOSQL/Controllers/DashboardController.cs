using IncidentManagementsSystemNOSQL.Models;
using IncidentManagementsSystemNOSQL.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IncidentManagementsSystemNOSQL.Controllers
{
    //for later
   // [Authorize(Roles = "service_desk")]
    public class DashboardController : Controller
    {
        private readonly ITicketService _ticketService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ITicketService ticketService, ILogger<DashboardController> logger)
        {
            _ticketService = ticketService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            try
            {
                var tickets = _ticketService.GetAll();
                var statusCounts = _ticketService.GetTicketCountsByStatus();
                var departmentCounts = _ticketService.GetTicketCountsByDepartment();

                ViewBag.StatusCounts = statusCounts;
                ViewBag.DepartmentCounts = departmentCounts;
                var totalTickets = tickets.Count;
                ViewBag.TotalTickets = totalTickets;

                var statusPercentages = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
                if (totalTickets > 0)
                {
                    foreach (var statusKey in statusCounts.Keys)
                    {
                        statusPercentages[statusKey] = Math.Round((double)statusCounts[statusKey] / totalTickets * 100, 1);
                    }
                }
                else
                {
                    statusPercentages = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
                }

                ViewBag.StatusPercentages = statusPercentages;

                return View(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading service desk dashboard");
                return StatusCode(500, "An error occurred while loading the dashboard.");
            }
        }
    }
}
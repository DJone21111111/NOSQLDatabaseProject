using IncidentManagementsSystemNOSQL.Models;
using IncidentManagementsSystemNOSQL.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IncidentManagementsSystemNOSQL.Controllers
{
    [Authorize(Roles = nameof(Enums.UserRole.employee))]
    public class EmployeeController : Controller
    {
        private readonly ITicketService _ticketService;
        private readonly IUserService _userService;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(ITicketService ticketService, IUserService userService, ILogger<EmployeeController> logger)
        {
            _ticketService = ticketService;
            _userService = userService;
            _logger = logger;
        }

        public IActionResult Index(string? employeeId)
        {
            try
            {
                ViewBag.HideUsersNav = true;
                ViewBag.EmployeeMode = true;

                var claimId = User.FindFirst("employee_id")?.Value;
                var effectiveEmployeeId = !string.IsNullOrWhiteSpace(claimId) ? claimId : employeeId;

                if (string.IsNullOrWhiteSpace(effectiveEmployeeId))
                {
                    TempData["ErrorMessage"] = "We could not find an employee profile to load.";
                    return View(new EmployeeDashboardViewModel());
                }

                User? employee = _userService.GetUserByEmployeeId(effectiveEmployeeId);
                if (employee == null)
                {
                    TempData["ErrorMessage"] = "We could not find an employee profile to load.";
                    return View(new EmployeeDashboardViewModel());
                }

                ViewBag.EmployeeId = employee.EmployeeId;

                List<Ticket> tickets = _ticketService.GetByUserId(employee.EmployeeId);
                Dictionary<Enums.TicketStatus, int> statusCounts = _ticketService.GetTicketCountsByStatusForEmployee(employee.EmployeeId);

                EmployeeDashboardViewModel model = BuildDashboardModel(employee, tickets, statusCounts);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading employee dashboard for {EmployeeId}", employeeId);
                TempData["ErrorMessage"] = "An unexpected error occurred while loading your dashboard.";
                return View(new EmployeeDashboardViewModel());
            }
        }

        public IActionResult CreateTicket()
        {
            try
            {
                ViewBag.HideUsersNav = true;
                ViewBag.EmployeeMode = true;

                var employeeId = User.FindFirst("employee_id")?.Value;
                if (string.IsNullOrWhiteSpace(employeeId))
                {
                    TempData["ErrorMessage"] = "We could not find your employee record.";
                    return RedirectToAction(nameof(Index));
                }

                User? employee = _userService.GetUserByEmployeeId(employeeId);
                if (employee == null)
                {
                    TempData["ErrorMessage"] = "We could not find your employee record.";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.EmployeeId = employee.EmployeeId;
                var model = new EmployeeTicketCreateViewModel
                {
                    EmployeeId = employee.EmployeeId,
                    EmployeeName = employee.Name,
                    EmployeeEmail = employee.Email,
                    DepartmentName = employee.Department?.Name ?? string.Empty,
                    DepartmentDescription = employee.Department?.Description ?? string.Empty
                };

                return View("CreateTicket", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preparing ticket creation form");
                TempData["ErrorMessage"] = "We could not load the ticket creation form. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public IActionResult CreateTicket(EmployeeTicketCreateViewModel model)
        {
            try
            {
                ViewBag.HideUsersNav = true;
                ViewBag.EmployeeMode = true;

                if (!ModelState.IsValid)
                {
                    return View("CreateTicket", model);
                }

                var employeeId = User.FindFirst("employee_id")?.Value;
                if (string.IsNullOrWhiteSpace(employeeId))
                {
                    ModelState.AddModelError(string.Empty, "We could not locate your employee profile. Please contact support.");
                    return View("CreateTicket", model);
                }

                User? employee = _userService.GetUserByEmployeeId(employeeId);
                if (employee == null)
                {
                    ModelState.AddModelError(string.Empty, "We could not locate your employee profile. Please contact support.");
                    return View("CreateTicket", model);
                }

                ViewBag.EmployeeId = employee.EmployeeId;

                var ticket = new Ticket
                {
                    Title = model.Title,
                    Description = model.Description,
                    Employee = new EmployeeEmbedded
                    {
                        EmployeeId = employee.EmployeeId,
                        Name = employee.Name,
                        Email = employee.Email,
                        Role = employee.Role.ToString(),
                        Department = new DepartmentEmbedded
                        {
                            DepartmentId = employee.Department?.DepartmentId,
                            Name = employee.Department?.Name ?? string.Empty,
                            Description = employee.Department?.Description ?? string.Empty
                        }
                    }
                };

                _ticketService.AddTicket(ticket);
                TempData["SuccessMessage"] = $"Ticket {ticket.TicketId} submitted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ticket");
                ModelState.AddModelError(string.Empty, "We could not submit your ticket right now. Please try again later.");
                return View("CreateTicket", model);
            }
        }

        private static EmployeeDashboardViewModel BuildDashboardModel(User employee, List<Ticket> tickets, Dictionary<Enums.TicketStatus, int> statusCounts)
        {
            statusCounts ??= new Dictionary<Enums.TicketStatus, int>();

            statusCounts.TryGetValue(Enums.TicketStatus.open, out int openCount);
            statusCounts.TryGetValue(Enums.TicketStatus.in_progress, out int inProgressCount);
            statusCounts.TryGetValue(Enums.TicketStatus.closed_resolved, out int closedResolvedCount);
            statusCounts.TryGetValue(Enums.TicketStatus.closed_no_resolve, out int closedNoResolveCount);

            return new EmployeeDashboardViewModel
            {
                EmployeeId = employee.EmployeeId,
                EmployeeName = employee.Name,
                EmployeeEmail = employee.Email,
                DepartmentName = employee.Department?.Name ?? string.Empty,
                DepartmentDescription = employee.Department?.Description ?? string.Empty,
                Tickets = tickets,
                TotalTickets = tickets.Count,
                OpenCount = openCount,
                InProgressCount = inProgressCount,
                ClosedResolvedCount = closedResolvedCount,
                ClosedNoResolveCount = closedNoResolveCount
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using IncidentManagementsSystemNOSQL.Models;
using IncidentManagementsSystemNOSQL.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IncidentManagementsSystemNOSQL.Controllers
{
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
// EMPLOYEE SELECTION LOGIC REPLACE WITH REAL AUTHENTICATION WHEN IMPLEMENTED
                User? employee = ResolveEmployee(employeeId);
                if (employee == null)
                {
                    TempData["ErrorMessage"] = "We could not find an employee profile to load.";
                    return View(new EmployeeDashboardViewModel());
                }

                ViewBag.EmployeeId = employee.EmployeeId;
                List<Ticket> tickets = _ticketService.GetByUserId(employee.EmployeeId);
                Dictionary<string, int> statusCounts = _ticketService.GetTicketCountsByStatusForEmployee(employee.EmployeeId);

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

        public IActionResult CreateTicket(string employeeId)
        {
            try
            {
                ViewBag.HideUsersNav = true;
                ViewBag.EmployeeMode = true;

                User? employee = _userService.GetUserByEmployeeId(employeeId);
                if (employee == null)
                {
                    TempData["ErrorMessage"] = "We could not find your employee record.";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.EmployeeId = employee.EmployeeId;
                EmployeeTicketCreateViewModel model = new EmployeeTicketCreateViewModel
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
                _logger.LogError(ex, "Error preparing ticket creation form for employee {EmployeeId}", employeeId);
                TempData["ErrorMessage"] = "We could not load the ticket creation form. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
        }

    [HttpPost]
    //[ValidateAntiForgeryToken] // TODO: re-enable once anti-forgery tokens are wired through employee create ticket flow
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

                User? employee = _userService.GetUserByEmployeeId(model.EmployeeId);
                if (employee == null)
                {
                    ModelState.AddModelError(string.Empty, "We could not locate your employee profile. Please contact support.");
                    return View("CreateTicket", model);
                }

                ViewBag.EmployeeId = employee.EmployeeId;
                Ticket ticket = new Ticket
                {
                    Title = model.Title,
                    Description = model.Description,
                    Employee = new EmployeeEmbedded
                    {
                        EmployeeId = employee.EmployeeId,
                        Name = employee.Name,
                        Email = employee.Email,
                        Role = employee.Role,
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
                return RedirectToAction(nameof(Index), new { employeeId = employee.EmployeeId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ticket for employee {EmployeeId}", model.EmployeeId);
                ModelState.AddModelError(string.Empty, "We could not submit your ticket right now. Please try again later.");
                return View("CreateTicket", model);
            }
        }

        // TODO: Replace this resolver with the authenticated user's identity when login/authorization is implemented
        private User? ResolveEmployee(string? employeeId)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(employeeId))
                {
                    User? user = _userService.GetUserByEmployeeId(employeeId);
                    if (user != null)
                    {
                        return user;
                    }
                }

                // Placeholder fallback: take the first user with role "employee" so the dashboard remains functional during development
                List<User> users = _userService.GetAllUsers();
                return users.FirstOrDefault(u => string.Equals(u.Role, "employee", StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resolve employee for id {EmployeeId}", employeeId);
                return null;
            }
        }

        private static EmployeeDashboardViewModel BuildDashboardModel(User employee, List<Ticket> tickets, Dictionary<string, int> statusCounts)
        {
            statusCounts ??= new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            statusCounts.TryGetValue("open", out int openCount);
            statusCounts.TryGetValue("in_progress", out int inProgressCount);
            statusCounts.TryGetValue("closed_resolved", out int closedResolvedCount);
            statusCounts.TryGetValue("closed_no_resolve", out int closedNoResolveCount);

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

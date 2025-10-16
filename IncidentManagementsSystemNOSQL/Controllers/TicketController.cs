using IncidentManagementsSystemNOSQL.Models;
using IncidentManagementsSystemNOSQL.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static IncidentManagementsSystemNOSQL.Models.Enums;

namespace IncidentManagementsSystemNOSQL.Controllers
{
    [Authorize(Roles = nameof(UserRole.service_desk))]
    public class TicketController : Controller
    {
        private readonly ITicketService _ticketService;
        private readonly ILogger<TicketController> _logger;

        public TicketController(ITicketService ticketService, ILogger<TicketController> logger)
        {
            _ticketService = ticketService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            try
            {
                List<Ticket> tickets = _ticketService.GetAll();
                return View(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tickets");
                return StatusCode(500, "An error occurred while retrieving tickets.");
            }
        }

        public IActionResult Details(string id)
        {
            try
            {
                Ticket? ticket = _ticketService.GetById(id);
                if (ticket == null)
                {
                    return NotFound();
                }
                return View(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading ticket {TicketId}", id);
                return StatusCode(500, "An error occurred while retrieving the ticket.");
            }
        }

        public IActionResult Create()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering create ticket view");
                return StatusCode(500, "An error occurred while preparing the ticket creation form.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Ticket ticket)
        {
            if (!ModelState.IsValid) return View(ticket);

            try
            {
                ticket.DateCreated = DateTime.UtcNow;
                ticket.Status = TicketStatus.open;

                _ticketService.AddTicket(ticket);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ticket for requester {Requester}", ticket?.Employee?.Email);
                ModelState.AddModelError("", "Unable to create ticket. Please try again.");
                return View(ticket);
            }
        }

        public IActionResult Edit(string id)
        {
            try
            {
                Ticket? ticket = _ticketService.GetById(id);
                if (ticket == null)
                {
                    return NotFound();
                }
                return View(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ticket {TicketId} for edit", id);
                return StatusCode(500, "An error occurred while retrieving the ticket.");
            }
        }

        [HttpPost]
        public IActionResult Edit(Ticket ticket)
        {
            _logger.LogInformation("Edit POST called with ticket.Id {TicketId}", ticket?.Id);

            if (ticket == null || string.IsNullOrWhiteSpace(ticket.Id))
            {
                _logger.LogWarning("Ticket payload was null or missing Id");
                TempData["ErrorMessage"] = "We couldn't find that ticket. Please try again.";
                return RedirectToAction("Index", "Dashboard");
            }

            try
            {
                Ticket? existingTicket = _ticketService.GetById(ticket.Id);
                if (existingTicket == null)
                {
                    _logger.LogWarning("Ticket {TicketId} not found in database", ticket.Id);
                    TempData["ErrorMessage"] = "Ticket no longer exists.";
                    return RedirectToAction("Index", "Dashboard");
                }

                ticket.TicketId = existingTicket.TicketId;
                ticket.Employee = existingTicket.Employee;
                ticket.AssignedTo = existingTicket.AssignedTo;
                ticket.Comments = existingTicket.Comments;
                ticket.DateCreated = existingTicket.DateCreated;
                ticket.DateClosed = existingTicket.DateClosed;

                if (ticket.Status == TicketStatus.closed_resolved || ticket.Status == TicketStatus.closed_no_resolve)
                {
                    ticket.DateClosed ??= DateTime.UtcNow;
                }
                else if (ticket.Status == TicketStatus.open || ticket.Status == TicketStatus.in_progress)
                {
                    ticket.DateClosed = null;
                }

                _logger.LogInformation("Updating ticket {TicketId} with status {Status}", ticket.Id, ticket.Status);
                _ticketService.UpdateTicket(ticket.Id, ticket);
                TempData["SuccessMessage"] = "Ticket updated successfully!";
                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ticket {TicketId}", ticket.Id);
                TempData["ErrorMessage"] = $"Unable to update ticket: {ex.Message}";
                ModelState.AddModelError("", "Unable to update ticket. Please try again.");
                return View(ticket);
            }
        }

        [HttpPost]
        public IActionResult CloseTicket([FromForm] string? id, [FromForm] string? reason)
        {
            _logger.LogInformation("CloseTicket called with id={TicketId}, reason={Reason}", id, reason);

            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("CloseTicket called with empty id parameter");
                TempData["ErrorMessage"] = "Invalid ticket ID";
                return RedirectToAction("Index", "Dashboard");
            }

            try
            {
                Ticket? ticket = _ticketService.GetById(id);
                if (ticket == null)
                {
                    _logger.LogWarning("Ticket with id {TicketId} not found when attempting to close", id);
                    TempData["ErrorMessage"] = "Ticket not found";
                    return RedirectToAction("Index", "Dashboard");
                }

                ticket.Status = (reason == "resolved" ? TicketStatus.closed_resolved : TicketStatus.closed_no_resolve);
                ticket.DateClosed = DateTime.UtcNow;

                _logger.LogInformation("Closing ticket {TicketNumber} with status {Status}", ticket.TicketId, ticket.Status);
                _ticketService.UpdateTicket(id, ticket);
                TempData["SuccessMessage"] = $"Ticket {ticket.TicketId} has been closed successfully!";
                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing ticket {TicketId}", id);
                TempData["ErrorMessage"] = $"Unable to close ticket: {ex.Message}";
                return RedirectToAction("Index", "Dashboard");
            }
        }

        public IActionResult Delete(string id)
        {
            try
            {
                Ticket? ticket = _ticketService.GetById(id);
                if (ticket == null)
                {
                    return NotFound();
                }
                return View(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ticket {TicketId} for deletion", id);
                return StatusCode(500, "An error occurred while retrieving the ticket.");
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            try
            {
                _ticketService.DeleteTicket(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting ticket {TicketId}", id);
                return StatusCode(500, "An error occurred while deleting the ticket.");
            }
        }
    }
}

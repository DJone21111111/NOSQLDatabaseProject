using IncidentManagementsSystemNOSQL.Models;
using IncidentManagementsSystemNOSQL.Repositories;
using IncidentManagementsSystemNOSQL.Service;
using Microsoft.AspNetCore.Mvc;

namespace IncidentManagementsSystemNOSQL.Controllers
{
    public class TicketController : Controller
    {
        private readonly ITicketService _ticketService;

        public TicketController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }
        public IActionResult Index()
        {
            try
            {
                var tickets = _ticketService.GetAll();
                return View(tickets);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading tickets: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving tickets.");
            }
        }

        public IActionResult Details(string id)
        {
            try
            {
                var ticket = _ticketService.GetById(id);
                if (ticket == null) return NotFound();
                return View(ticket);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading ticket {id}: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving the ticket.");
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Ticket ticket)
        {
            if (!ModelState.IsValid) return View(ticket);

            try
            {
                ticket.DateCreated = DateTime.UtcNow;
                ticket.Status = "open";

                _ticketService.AddTicket(ticket);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating ticket: {ex.Message}");
                ModelState.AddModelError("", "Unable to create ticket. Please try again.");
                return View(ticket);
            }
        }

        public IActionResult Edit(string id)
        {
            try
            {

                var ticket = _ticketService.GetById(id);
                if (ticket == null) return NotFound();
                return View(ticket);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving ticket {id} for edit: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving the ticket.");
            }
        }

    [HttpPost]
        public IActionResult Edit(Ticket ticket)
        {
            Console.WriteLine($"Edit POST called with ticket.Id: {ticket?.Id}");

            if (ticket == null || string.IsNullOrWhiteSpace(ticket.Id))
            {
                Console.WriteLine("Ticket payload was null or missing Id");
                TempData["ErrorMessage"] = "We couldn't find that ticket. Please try again.";
                return RedirectToAction("Index", "Dashboard");
            }

            try
            {
                var existingTicket = _ticketService.GetById(ticket.Id);
                if (existingTicket == null)
                {
                    Console.WriteLine($"Ticket {ticket.Id} not found in database");
                    TempData["ErrorMessage"] = "Ticket no longer exists.";
                    return RedirectToAction("Index", "Dashboard");
                }

                // Copy immutable fields so we don't rely on hidden inputs
                ticket.TicketId = existingTicket.TicketId;
                ticket.Employee = existingTicket.Employee;
                ticket.AssignedTo = existingTicket.AssignedTo;
                ticket.Comments = existingTicket.Comments;
                ticket.DateCreated = existingTicket.DateCreated;

                // Keep original DateClosed unless we are changing status
                ticket.DateClosed = existingTicket.DateClosed;

                // Automatically set DateClosed if status is being changed to closed
                if ((ticket.Status == "closed_resolved" || ticket.Status == "closed_no_resolve"))
                {
                    ticket.DateClosed ??= DateTime.UtcNow;
                }
                else if (ticket.Status == "open" || ticket.Status == "in_progress")
                {
                    ticket.DateClosed = null;
                }

                Console.WriteLine($"Updating ticket {ticket.Id} with status: {ticket.Status}");
                _ticketService.UpdateTicket(ticket.Id, ticket);
                TempData["SuccessMessage"] = "Ticket updated successfully!";
                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating ticket {ticket.Id}: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = $"Unable to update ticket: {ex.Message}";
                ModelState.AddModelError("", "Unable to update ticket. Please try again.");
                return View(ticket);
            }
        }

        // Quick action to close a ticket
    [HttpPost]
        public IActionResult CloseTicket([FromForm] string? id, [FromForm] string? reason)
        {
            Console.WriteLine($"CloseTicket called with id={id}, reason={reason}");
            
            if (string.IsNullOrEmpty(id))
            {
                Console.WriteLine("ERROR: id parameter is null or empty");
                TempData["ErrorMessage"] = "Invalid ticket ID";
                return RedirectToAction("Index", "Dashboard");
            }
            
            try
            {
                var ticket = _ticketService.GetById(id);
                if (ticket == null)
                {
                    Console.WriteLine($"ERROR: Ticket with id {id} not found");
                    TempData["ErrorMessage"] = "Ticket not found";
                    return RedirectToAction("Index", "Dashboard");
                }

                // Set ticket to closed
                ticket.Status = reason == "resolved" ? "closed_resolved" : "closed_no_resolve";
                ticket.DateClosed = DateTime.UtcNow;
                
                Console.WriteLine($"Closing ticket {ticket.TicketId} with status {ticket.Status}");
                _ticketService.UpdateTicket(id, ticket);
                TempData["SuccessMessage"] = $"Ticket {ticket.TicketId} has been closed successfully!";
                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error closing ticket {id}: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = $"Unable to close ticket: {ex.Message}";
                return RedirectToAction("Index", "Dashboard");
            }
        }

        public IActionResult Delete(string id)
        {
            try
            {
                var ticket = _ticketService.GetById(id);
                if (ticket == null) return NotFound();
                return View(ticket);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving ticket {id} for deletion: {ex.Message}");
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
                Console.WriteLine($"Error deleting ticket {id}: {ex.Message}");
                return StatusCode(500, "An error occurred while deleting the ticket.");
            }
        }

       
    }
}

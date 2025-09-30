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

        public async Task<IActionResult> Index()
        {
            try
            {
                var tickets = await _ticketService.GetAllTickets();
                return View(tickets);
            }
            catch (Exception ex)
            {
                // Log error (you can inject ILogger<TicketController> for proper logging)
                Console.WriteLine($"Error loading tickets: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving tickets.");
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            try
            {
                var ticket = await _ticketService.GetTicketById(id);
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
        public async Task<IActionResult> Create(Ticket ticket)
        {
            if (!ModelState.IsValid) return View(ticket);

            try
            {
                ticket.DateCreated = DateTime.UtcNow;
                ticket.Status = Enums.TicketStatus.open;

                await _ticketService.AddTicket(ticket);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating ticket: {ex.Message}");
                ModelState.AddModelError("", "Unable to create ticket. Please try again.");
                return View(ticket);
            }
        }

        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var ticket = await _ticketService.GetTicketById(id);
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Ticket ticket)
        {
            if (id != ticket.Id) return BadRequest();
            if (!ModelState.IsValid) return View(ticket);

            try
            {
                await _ticketService.UpdateTicket(id,ticket);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating ticket {id}: {ex.Message}");
                ModelState.AddModelError("", "Unable to update ticket. Please try again.");
                return View(ticket);
            }
        }

        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var ticket = await _ticketService.GetTicketById(id);
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
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                await _ticketService.DeleteTicket(id);
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

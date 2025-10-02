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
                ticket.Status = Enums.TicketStatus.open;

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
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string id, Ticket ticket)
        {
            if (id != ticket.Id) return BadRequest();
            if (!ModelState.IsValid) return View(ticket);

            try
            {
                _ticketService.UpdateTicket(id, ticket);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating ticket {id}: {ex.Message}");
                ModelState.AddModelError("", "Unable to update ticket. Please try again.");
                return View(ticket);
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

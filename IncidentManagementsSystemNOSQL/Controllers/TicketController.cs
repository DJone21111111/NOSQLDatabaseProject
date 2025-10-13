using IncidentManagementsSystemNOSQL.Models;
using IncidentManagementsSystemNOSQL.Repositories;
using IncidentManagementsSystemNOSQL.Service;
using IncidentManagementsSystemNOSQL.Service.IncidentManagementsSystemNOSQL.Service;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace IncidentManagementsSystemNOSQL.Controllers
{
    public class TicketController : Controller
    {
        private readonly ITicketService _ticketService;
        private readonly ITicketPriorityService _priorityService

            ;

        public TicketController(ITicketService ticketService, ITicketPriorityService priorityService)
        {
            _ticketService = ticketService;
            _priorityService = _priorityService;
        }
        public IActionResult Index([FromQuery] string? priority = null)
        {
            // TEMP: no try/catch so you get the full yellow-page stack in dev
            if (!string.IsNullOrWhiteSpace(priority) &&
                _priorityService.TryParsePriority(priority, out var p))
            {
                var filtered = _priorityService.BuildPriorityFilter(p);
                ViewBag.Priority = p.ToString();
                return View(filtered);
            }

            ViewBag.Priority = "All";
            return View(_ticketService.GetAll());
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
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string id, Ticket ticket)
        {
            if (!ObjectId.TryParse(id, out var objectId) || objectId != ticket.Id)
                return BadRequest();

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

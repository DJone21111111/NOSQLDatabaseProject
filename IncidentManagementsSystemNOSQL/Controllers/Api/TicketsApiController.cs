using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using IncidentManagementsSystemNOSQL.Models;
using IncidentManagementsSystemNOSQL.Models.Api;
using IncidentManagementsSystemNOSQL.Service;

namespace IncidentManagementsSystemNOSQL.Controllers.Api
{
    [ApiController]
    [Route("api/v1/tickets")]
    public class TicketsApiController : ControllerBase
    {
        private readonly ITicketService _ticketService;
        private readonly ILogger<TicketsApiController> _logger;

        public TicketsApiController(ITicketService ticketService, ILogger<TicketsApiController> logger)
        {
            _ticketService = ticketService;
            _logger = logger;
        }

    /// <summary>
    /// Retrieves the full collection of tickets available in the system.
    /// </summary>
    /// <returns>A list of <see cref="TicketDto"/> records.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<IEnumerable<TicketDto>> GetAll()
        {
            try
            {
                List<Ticket> tickets = _ticketService.GetAll();
                List<TicketDto> response = new List<TicketDto>();
                foreach (Ticket ticket in tickets)
                {
                    response.Add(MapToTicketDto(ticket));
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve tickets");
                return Problem(detail: "An unexpected error occurred while listing tickets.", statusCode: 500);
            }
        }

    /// <summary>
    /// Retrieves a single ticket using its MongoDB identifier.
    /// </summary>
    /// <param name="id">The MongoDB document identifier of the ticket.</param>
    /// <returns>The matching <see cref="TicketDto"/> when found.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<TicketDto> GetById(string id)
        {
            try
            {
                Ticket? ticket = _ticketService.GetById(id);
                if (ticket == null)
                {
                    return NotFound();
                }

                return Ok(MapToTicketDto(ticket));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve ticket {TicketId}", id);
                return Problem(detail: "An unexpected error occurred while retrieving the ticket.", statusCode: 500);
            }
        }

    /// <summary>
    /// Creates a new ticket for the specified reporter.
    /// </summary>
    /// <param name="request">The ticket information to persist.</param>
    /// <returns>The created <see cref="TicketDto"/> with identifiers populated.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<TicketDto> Create([FromBody] TicketCreateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (request.Reporter == null || request.Reporter.Department == null)
            {
                ModelState.AddModelError(nameof(request.Reporter), "Reporter details are required.");
                return ValidationProblem(ModelState);
            }

            try
            {
                Ticket ticket = MapToTicket(request);
                _ticketService.AddTicket(ticket);

                TicketDto dto = MapToTicketDto(ticket);
                return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create ticket for reporter {Reporter}", request.Reporter.EmployeeId);
                return Problem(detail: "An unexpected error occurred while creating the ticket.", statusCode: 500);
            }
        }

    /// <summary>
    /// Updates an existing ticket with new title, description, or status details.
    /// </summary>
    /// <param name="id">The MongoDB document identifier of the ticket.</param>
    /// <param name="request">The fields to update on the ticket.</param>
    /// <returns>The refreshed <see cref="TicketDto"/> after persistence.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<TicketDto> Update(string id, [FromBody] TicketUpdateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            try
            {
                Ticket? existingTicket = _ticketService.GetById(id);
                if (existingTicket == null)
                {
                    return NotFound();
                }

                existingTicket.Title = request.Title;
                existingTicket.Description = request.Description;
                existingTicket.Status = request.Status;

                if (request.Status == "closed_resolved" || request.Status == "closed_no_resolve")
                {
                    existingTicket.DateClosed = DateTime.UtcNow;
                }
                else if (request.Status == "open" || request.Status == "in_progress")
                {
                    existingTicket.DateClosed = null;
                }

                _ticketService.UpdateTicket(id, existingTicket);
                TicketDto dto = MapToTicketDto(existingTicket);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update ticket {TicketId}", id);
                return Problem(detail: "An unexpected error occurred while updating the ticket.", statusCode: 500);
            }
        }

    /// <summary>
    /// Deletes a ticket from the system.
    /// </summary>
    /// <param name="id">The MongoDB document identifier of the ticket.</param>
    /// <returns>No content when the ticket is successfully removed.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Delete(string id)
        {
            try
            {
                Ticket? existingTicket = _ticketService.GetById(id);
                if (existingTicket == null)
                {
                    return NotFound();
                }

                _ticketService.DeleteTicket(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete ticket {TicketId}", id);
                return Problem(detail: "An unexpected error occurred while deleting the ticket.", statusCode: 500);
            }
        }

        private static TicketDto MapToTicketDto(Ticket ticket)
        {
            TicketReporterDto reporter = new TicketReporterDto
            {
                EmployeeId = ticket.Employee?.EmployeeId ?? string.Empty,
                Name = ticket.Employee?.Name ?? string.Empty,
                Email = ticket.Employee?.Email ?? string.Empty,
                Role = ticket.Employee?.Role ?? string.Empty,
                DepartmentName = ticket.Employee?.Department?.Name ?? string.Empty,
                DepartmentDescription = ticket.Employee?.Department?.Description ?? string.Empty
            };

            TicketAssignmentDto? assignment = null;
            if (ticket.AssignedTo != null)
            {
                assignment = new TicketAssignmentDto
                {
                    EmployeeId = ticket.AssignedTo.EmployeeId ?? string.Empty,
                    Name = ticket.AssignedTo.Name ?? string.Empty,
                    Email = ticket.AssignedTo.Email ?? string.Empty,
                    Role = ticket.AssignedTo.Role ?? string.Empty
                };
            }

            TicketDto dto = new TicketDto
            {
                Id = ticket.Id ?? string.Empty,
                TicketId = ticket.TicketId ?? string.Empty,
                Title = ticket.Title ?? string.Empty,
                Description = ticket.Description,
                Status = ticket.Status ?? string.Empty,
                DateCreatedUtc = ticket.DateCreated,
                DateClosedUtc = ticket.DateClosed,
                Reporter = reporter,
                AssignedTo = assignment
            };

            return dto;
        }

        private static Ticket MapToTicket(TicketCreateRequest request)
        {
            DepartmentEmbedded department = new DepartmentEmbedded
            {
                DepartmentId = request.Reporter.Department.DepartmentId,
                Name = request.Reporter.Department.Name,
                Description = request.Reporter.Department.Description
            };

            EmployeeEmbedded employee = new EmployeeEmbedded
            {
                EmployeeId = request.Reporter.EmployeeId,
                Name = request.Reporter.Name,
                Email = request.Reporter.Email,
                Role = request.Reporter.Role,
                Department = department
            };

            Ticket ticket = new Ticket
            {
                Title = request.Title,
                Description = request.Description,
                Employee = employee
            };

            return ticket;
        }
    }
}

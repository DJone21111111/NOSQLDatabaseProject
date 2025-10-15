using System;

namespace IncidentManagementsSystemNOSQL.Models.Api
{
    public class TicketDto
    {
        public string Id { get; set; } = string.Empty;
        public string TicketId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime DateCreatedUtc { get; set; }
        public DateTime? DateClosedUtc { get; set; }
        public TicketReporterDto Reporter { get; set; } = new TicketReporterDto();
        public TicketAssignmentDto? AssignedTo { get; set; }
    }

    public class TicketReporterDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string DepartmentDescription { get; set; } = string.Empty;
    }

    public class TicketAssignmentDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}

using System.ComponentModel.DataAnnotations;

namespace IncidentManagementsSystemNOSQL.Models.Api
{
    public class TicketCreateRequest
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(4000)]
        public string? Description { get; set; }

        [Required]
        public TicketReporterRequest Reporter { get; set; } = new TicketReporterRequest();
    }

    public class TicketReporterRequest
    {
        [Required]
        [StringLength(32)]
        public string EmployeeId { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = "employee";

        [Required]
        public TicketReporterDepartmentRequest Department { get; set; } = new TicketReporterDepartmentRequest();
    }

    public class TicketReporterDepartmentRequest
    {
        [StringLength(32)]
        public string? DepartmentId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(400)]
        public string Description { get; set; } = string.Empty;
    }
}

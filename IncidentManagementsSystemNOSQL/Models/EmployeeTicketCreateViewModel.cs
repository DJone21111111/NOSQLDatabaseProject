using System.ComponentModel.DataAnnotations;

namespace IncidentManagementsSystemNOSQL.Models
{
    public class EmployeeTicketCreateViewModel
    {
        [Required]
        public string EmployeeId { get; set; } = string.Empty;

        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeEmail { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string DepartmentDescription { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Issue title")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 150 characters.")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Describe the problem")]
        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
        public string? Description { get; set; }
    }
}

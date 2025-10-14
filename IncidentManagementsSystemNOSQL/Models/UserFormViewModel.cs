using System.ComponentModel.DataAnnotations;

namespace IncidentManagementsSystemNOSQL.Models
{
    public class UserFormViewModel
    {
        public string? Id { get; set; }

        [Required]
        [Display(Name = "Employee ID")]
        public string EmployeeId { get; set; } = null!;

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string Role { get; set; } = "employee";

        [Required]
        [Display(Name = "Department Name")]
        public string DepartmentName { get; set; } = null!;

        [Display(Name = "Department Description")]
        public string DepartmentDescription { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Username")]
        public string UserName { get; set; } = null!;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Must Change Password")]
        public bool MustChangePassword { get; set; }

        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string? ConfirmPassword { get; set; }

        public string? LastUpdatedDisplay { get; set; }
    }
}

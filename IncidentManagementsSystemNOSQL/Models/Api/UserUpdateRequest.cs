using System.ComponentModel.DataAnnotations;

namespace IncidentManagementsSystemNOSQL.Models.Api
{
    public class UserUpdateRequest
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(32)]
        public string Role { get; set; } = "employee";

        [Required]
        [StringLength(200)]
        public string DepartmentName { get; set; } = string.Empty;

        [StringLength(400)]
        public string DepartmentDescription { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string UserName { get; set; } = string.Empty;

        [StringLength(200, MinimumLength = 6)]
        public string? Password { get; set; }

        public bool IsActive { get; set; } = true;
        public bool MustChangePassword { get; set; } = true;
    }
}

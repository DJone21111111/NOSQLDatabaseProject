namespace IncidentManagementsSystemNOSQL.Models
{
    public class User
    {

        // User Identity
        public string Id { get; set; } = null!;
        public string EmployeeId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public Enums.UserRole Role { get; set; }

        // User Department

        public DepartmentEmbedded Department { get; set; } = null!;

        // user Account

        public bool IsActive { get; set; } = true;
        public string UserName { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;

        // Forcing user to reset password on first login.
        public bool MustChangePassword { get; set; } = false;

        // Timestamps (We know when the user was created and last updated)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

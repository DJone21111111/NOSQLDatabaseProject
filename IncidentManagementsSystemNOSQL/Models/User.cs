using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace IncidentManagementsSystemNOSQL.Models
{
    [BsonIgnoreExtraElements]
    public class User
    {

        // User Identity
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("employeeId")]
        public string EmployeeId { get; set; } = null!;

        [BsonElement("name")]
        public string Name { get; set; } = null!;

        [BsonElement("email")]
        public string Email { get; set; } = null!;

        [BsonElement("role")]
        public Enums.UserRole Role { get; set; }

        // User Department
        [BsonElement("department")]
        public DepartmentEmbedded Department { get; set; } = null!;

        // user Account
        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        [BsonElement("username")]
        public string UserName { get; set; } = null!;

        [BsonElement("passwordHash")]
        public string PasswordHash { get; set; } = null!;

        // Forcing user to reset password on first login.
        [BsonElement("mustChangePassword")]
        public bool MustChangePassword { get; set; } = false;

        // Timestamps (We know when the user was created and last updated)
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

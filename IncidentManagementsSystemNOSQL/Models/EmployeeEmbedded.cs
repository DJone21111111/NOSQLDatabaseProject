using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace IncidentManagementsSystemNOSQL.Models
{
    public class EmployeeEmbedded
    {
        public string EmployeeId { get; set; } = null!;
        public string Name { get; set; } = null!;

        public string Email { get; set; } = null!;

        public Enums.UserRole Role { get; set; }

        public DepartmentEmbedded Department { get; set; } = null!;
    }
}

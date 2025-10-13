using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace IncidentManagementsSystemNOSQL.Models
{
    [BsonIgnoreExtraElements]
    public class EmployeeEmbedded
    {
        [BsonElement("employeeId")]
        public string EmployeeId { get; set; } = null!;
        
        [BsonElement("name")]
        public string Name { get; set; } = null!;

        [BsonElement("email")]
        public string Email { get; set; } = null!;

        [BsonElement("role")]
        public string Role { get; set; } = "employee";

        [BsonElement("department")]
        public DepartmentEmbedded Department { get; set; } = null!;
    }
}

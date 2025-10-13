using MongoDB.Bson.Serialization.Attributes;

namespace IncidentManagementsSystemNOSQL.Models
{
    [BsonIgnoreExtraElements]
    public class CommentAuthorEmbedded
    {
        [BsonElement("employeeId")]
        public string EmployeeId { get; set; } = null!;
        
        [BsonElement("name")]
        public string Name { get; set; } = null!;
        
        [BsonElement("email")]
        public string? Email { get; set; }
        
        [BsonElement("role")]
        public string? Role { get; set; }


    }
}

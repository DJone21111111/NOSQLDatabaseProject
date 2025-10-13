using MongoDB.Bson.Serialization.Attributes;

namespace IncidentManagementsSystemNOSQL.Models
{
    [BsonIgnoreExtraElements]
    public class DepartmentEmbedded
    {
        [BsonElement("departmentId")]
        public string DepartmentId { get; set; } = null!;
        
        [BsonElement("name")]
        public string Name { get; set; } = null!;
        
        [BsonElement("description")]
        public string Description { get; set; } = null!;

    }
}

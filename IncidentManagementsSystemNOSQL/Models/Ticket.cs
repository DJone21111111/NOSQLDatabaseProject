using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace IncidentManagementsSystemNOSQL.Models
{
    [BsonIgnoreExtraElements]
    public class Ticket
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        
        [BsonElement("TicketId")]
        public string TicketId { get; set; } = null!;
        
        [BsonElement("Title")]
        public string Title { get; set; } = null!;
        
        [BsonElement("Description")]
        public string? Description { get; set; }

        [BsonElement("Status")]
        public string Status { get; set; } = "open";
        
        [BsonElement("DateCreated")]
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        
        [BsonElement("DateClosed")]
        public DateTime? DateClosed { get; set; }
        
        [BsonElement("Employee")]
        public EmployeeEmbedded Employee { get; set; } = null!;

        [BsonElement("HandledBy")]
        public List<CommentAuthorEmbedded>? AssignedAgents { get; set; }

        [BsonIgnore]
        public CommentAuthorEmbedded? AssignedTo
        {
            get => AssignedAgents?.FirstOrDefault();
            set
            {
                if (value == null)
                {
                    AssignedAgents = null;
                    return;
                }

                AssignedAgents = new List<CommentAuthorEmbedded> { value };
            }
        }

        [BsonElement("Comments")]
        public List<TicketComment> Comments { get; set; } = new();
    }

}

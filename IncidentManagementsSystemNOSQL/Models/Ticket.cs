using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace IncidentManagementsSystemNOSQL.Models
{
    public class Ticket
    {
        public ObjectId Id { get; set; }
        public string TicketId { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Description { get; set; }

        public Enums.PriorityLevel Priority { get; set; } = Enums.PriorityLevel.low;

        public Enums.TicketStatus Status { get; set; } = Enums.TicketStatus.open;
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime? DateClosed { get; set; }
        public EmployeeEmbedded Employee { get; set; } = null!;

        public CommentAuthorEmbedded? AssignedTo { get; set; }

        public List<TicketComment> Comments { get; set; } = new();
    }

}

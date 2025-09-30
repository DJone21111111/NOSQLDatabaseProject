using MongoDB.Bson.Serialization.Attributes;

namespace IncidentManagementsSystemNOSQL.Models
{
    public class TicketComment
    {
        public string CommentId { get; set; } = null!;

        public CommentAuthorEmbedded Author { get; set; } = null!;

        public string Text { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

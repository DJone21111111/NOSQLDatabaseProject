using MongoDB.Bson.Serialization.Attributes;

namespace IncidentManagementsSystemNOSQL.Models
{
    [BsonIgnoreExtraElements]
    public class TicketComment
    {
        [BsonElement("commentId")]
        public string CommentId { get; set; } = null!;

        [BsonElement("author")]
        public CommentAuthorEmbedded Author { get; set; } = null!;

        [BsonElement("text")]
        public string Text { get; set; } = null!;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace IncidentManagementsSystemNOSQL.Models.ViewModels
{
    public class PasswordResetToken
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string UserId { get; set; }
        public string TokenHash { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
        public bool Used { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public string IssuerIp { get; set; }
    }
}

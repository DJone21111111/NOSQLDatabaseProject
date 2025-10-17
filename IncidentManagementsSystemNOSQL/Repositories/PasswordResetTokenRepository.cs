using IncidentManagementsSystemNOSQL.Models;
using IncidentManagementsSystemNOSQL.Models.ViewModels;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace IncidentManagementsSystemNOSQL.Repositories
{
    public class PasswordResetTokenRepository : IPasswordResetTokenRepository
    {
        private readonly IMongoCollection<PasswordResetToken> _col;

        public PasswordResetTokenRepository(IOptions<MongoDbSettings> options)
        {
            var client = new MongoClient(options.Value.ConnectionString);
            var db = client.GetDatabase(options.Value.DatabaseName);
            _col = db.GetCollection<PasswordResetToken>("password_reset_tokens");
        }

        public void EnsureIndexes()
        {
            var existing = _col.Indexes.List().ToList().Select(b => b["name"].AsString).ToHashSet();

            if (!existing.Contains("ExpiresAtUtc_1"))
            {
                var ttl = new CreateIndexModel<PasswordResetToken>(
                    Builders<PasswordResetToken>.IndexKeys.Ascending(x => x.ExpiresAtUtc),
                    new CreateIndexOptions { Name = "ExpiresAtUtc_1", ExpireAfter = TimeSpan.Zero });
                _col.Indexes.CreateOne(ttl);
            }

            if (existing.Contains("UserId_1_TokenHash_1"))
                _col.Indexes.DropOne("UserId_1_TokenHash_1");

            var options = new CreateIndexOptions<PasswordResetToken>
            {
                Name = "UserId_1_TokenHash_1",
                Unique = true,
                Sparse = true
            };

            var lookup = new CreateIndexModel<PasswordResetToken>(
                Builders<PasswordResetToken>.IndexKeys
                    .Ascending(x => x.UserId)
                    .Ascending(x => x.TokenHash),
                options);

            _col.Indexes.CreateOne(lookup);
        }

        public void Insert(PasswordResetToken token)
        {
            if (string.IsNullOrWhiteSpace(token.Id))
                token.Id = ObjectId.GenerateNewId().ToString();
            if (string.IsNullOrWhiteSpace(token.UserId) || string.IsNullOrWhiteSpace(token.TokenHash))
                throw new ArgumentException("UserId and TokenHash are required.");
            _col.InsertOne(token);
        }

        public PasswordResetToken FindActiveByUserAndHash(string userId, string tokenHash)
        {
            var now = DateTime.UtcNow;
            return _col.Find(x => x.UserId == userId &&
                                  x.TokenHash == tokenHash &&
                                  !x.Used &&
                                  x.ExpiresAtUtc > now)
                       .FirstOrDefault();
        }

        public void MarkAllActiveAsUsed(string userId)
        {
            var now = DateTime.UtcNow;
            var filter = Builders<PasswordResetToken>.Filter.Where(t => t.UserId == userId && !t.Used && t.ExpiresAtUtc > now);
            var update = Builders<PasswordResetToken>.Update.Set(t => t.Used, true);
            _col.UpdateMany(filter, update);
        }

        public void Update(PasswordResetToken token)
        {
            _col.ReplaceOne(t => t.Id == token.Id, token);
        }
    }
}

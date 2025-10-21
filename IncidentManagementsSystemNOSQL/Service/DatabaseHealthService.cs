using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace IncidentManagementsSystemNOSQL.Service
{
    public sealed class MongoHealthResult
    {
        public MongoHealthResult(bool isHealthy, IReadOnlyList<string> collections)
        {
            IsHealthy = isHealthy;
            Collections = collections;
        }

        public bool IsHealthy { get; }

        public IReadOnlyList<string> Collections { get; }
    }

    public interface IDatabaseHealthService
    {
        MongoHealthResult CheckMongoHealth();
    }

    public sealed class DatabaseHealthService : IDatabaseHealthService
    {
        private readonly IMongoDatabase _database;
        private readonly ILogger<DatabaseHealthService> _logger;

        public DatabaseHealthService(IMongoDatabase database, ILogger<DatabaseHealthService> logger)
        {
            _database = database;
            _logger = logger;
        }

        public MongoHealthResult CheckMongoHealth()
        {
            try
            {
                BsonDocument pingCommand = new BsonDocument("ping", 1);
                _database.RunCommand<BsonDocument>(pingCommand);

                List<string> collections = _database.ListCollectionNames().ToList();
                return new MongoHealthResult(true, collections);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to run MongoDB health check");
                string[] emptyCollections = Array.Empty<string>();
                return new MongoHealthResult(false, emptyCollections);
            }
        }
    }
}

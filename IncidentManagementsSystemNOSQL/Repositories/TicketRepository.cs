using IncidentManagementsSystemNOSQL.Models;
using IncidentManagementsSystemNOSQL.Service.IncidentManagementsSystemNOSQL.Service;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace IncidentManagementsSystemNOSQL.Repositories
{
    public class TicketRepository : ITicketRepository
    {
        private readonly IMongoCollection<Ticket> _tickets;
        private readonly ITicketPriorityService _prioritySvc;

        public TicketRepository(IMongoDatabase db)
        {
            _tickets = db.GetCollection<Ticket>("tickets");
            _prioritySvc = _prioritySvc;
        }
        [BsonElement("ticketId")]
        public string TicketId { get; set; } = null!;

        public Ticket? GetById(string id)
        {
            try
            {
                var objectId = ObjectId.Parse(id); // Convert string to ObjectId
                return _tickets.Find(t => t.Id == id).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while retrieving ticket with id '{id}'", ex);
            }
        }

        public List<Ticket> GetByUserId(string userId)
        {
            try
            {
                return _tickets.Find(t => t.Employee.EmployeeId == userId).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while retrieving tickets for user '{userId}'", ex);
            }
        }

        public List<Ticket> GetAll()
        {
            try
            {
                return _tickets.Find(FilterDefinition<Ticket>.Empty)
                                 .SortByDescending(t => t.DateCreated)
                                 .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while retrieving all tickets", ex);
            }
        }

        public List<Ticket> GetByStatus(string status)
        {
            try
            {
                return _tickets.Find(t => t.Status == status).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while retrieving tickets with status '{status}'", ex);
            }
        }

        public List<Ticket> GetByDateRange(DateTime startDate, DateTime endDate)
        {
            try
            {
                // Filters for tickets created between the start and end dates (inclusive)
                return _tickets.Find(t => t.DateCreated >= startDate && t.DateCreated <= endDate)
                                 .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while retrieving tickets by date range", ex);
            }
        }

        public void AddTicket(Ticket ticket)
        {
            try
            {
                _tickets.InsertOne(ticket);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while adding new ticket", ex);
            }
        }

        public void UpdateTicket(string id, Ticket updated)
        {
            try
            {
                var objectId = ObjectId.Parse(id); // Convert string to ObjectId
                _tickets.ReplaceOne(t => t.Id == id, updated);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while updating ticket '{id}'", ex);
            }
        }

        public void DeleteById(string id)
        {
            try
            {
                var objectId = ObjectId.Parse(id); // Convert string to ObjectId
                _tickets.DeleteOne(t => t.Id == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while deleting ticket with id '{id}'", ex);
            }
        }

        public Dictionary<string, int> GetTicketCountsByStatus()
        {
            try
            {
                var results = _tickets.Aggregate()
                    .Group(t => t.Status, g => new { Status = g.Key, Count = g.Count() })
                    .ToList();

                return results.ToDictionary(r => r.Status, r => r.Count);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while aggregating tickets by status", ex);
            }
        }

        public Dictionary<string, int> GetTicketCountsByDepartment()
        {
            try
            {
                var results = _tickets.Aggregate()
                    .Group(t => t.Employee.Department.Name, g => new { Department = g.Key, Count = g.Count() })
                    .ToList();

                return results.ToDictionary(r => r.Department, r => r.Count);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while aggregating tickets by department", ex);
            }
        }

        // Makes query faster and ensures uniqueness
        public void EnsureIndexes()
        {
            try
            {
                var models = new[]
                {
                    new CreateIndexModel<Ticket>(
                        Builders<Ticket>.IndexKeys.Ascending(t => t.Status),
                        new CreateIndexOptions { Name = "ix_status" }),

                    new CreateIndexModel<Ticket>(
                        Builders<Ticket>.IndexKeys.Ascending("Employee.EmployeeId"),
                        new CreateIndexOptions { Name = "ix_employeeId" }),

                    new CreateIndexModel<Ticket>(
                        Builders<Ticket>.IndexKeys.Descending(t => t.DateCreated),
                        new CreateIndexOptions { Name = "ix_createdAt" })
                };

                _tickets.Indexes.CreateManyAsync(models).Wait();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while ensuring indexes for tickets collection", ex);
            }
        }

        public List<Ticket> GetByPriority(Enums.PriorityLevel priority)
        {
            var filter = _prioritySvc.BuildPriorityFilter(priority);

            // Optional: order newest first within this priority
            var sort = Builders<Ticket>.Sort.Descending(t => t.DateCreated);

            return _tickets.Find(filter)
                           .Sort(sort)
                           .Limit(1000)
                           .ToList();
        }
    }
}

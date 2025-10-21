using IncidentManagementsSystemNOSQL.Models;
using MongoDB.Driver;
using static IncidentManagementsSystemNOSQL.Models.Enums;

namespace IncidentManagementsSystemNOSQL.Repositories
{
    public class TicketRepository : ITicketRepository
    {
        private readonly IMongoCollection<Ticket> _tickets;
        private readonly IMongoCollection<Counter> _counters;
        private const string TicketCounterId = "ticket-sequence";
        private const string ServiceDeskRotationCounterId = "service-desk-rotation";

        public TicketRepository(IMongoDatabase db)
        {
            _tickets = db.GetCollection<Ticket>("tickets");
            _counters = db.GetCollection<Counter>("counters");
        }

        public Ticket? GetById(string id)
        {
            return _tickets.Find(t => t.Id == id).FirstOrDefault();
        }

        public List<Ticket> GetByUserId(string userId)
        {
            return _tickets.Find(t => t.Employee.EmployeeId == userId).ToList();
        }

        public List<Ticket> GetAll()
        {
            return _tickets.Find(FilterDefinition<Ticket>.Empty)
                           .SortByDescending(t => t.DateCreated)
                           .ToList();
        }

        public List<Ticket> GetByStatus(TicketStatus status)
        {
            return _tickets.Find(t => t.Status == status).ToList();
        }

        public List<Ticket> GetByDateRange(DateTime startDate, DateTime endDate)
        {
            return _tickets.Find(t => t.DateCreated >= startDate && t.DateCreated <= endDate).ToList();
        }

        public void AddTicket(Ticket ticket)
        {
            _tickets.InsertOne(ticket);
        }

        public void UpdateTicket(string id, Ticket updated)
        {
            _tickets.ReplaceOne(t => t.Id == id, updated);
        }

        public void DeleteById(string id)
        {
            _tickets.DeleteOne(t => t.Id == id);
        }

        public Dictionary<Enums.TicketStatus, int> GetTicketCountsByStatus()
        {
            var allTickets = GetAll(); 

            var groupedCounts = allTickets
                .GroupBy(t => t.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            return groupedCounts;
        }


        public Dictionary<DepartmentType, int> GetTicketCountsByDepartment()
        {
            var results = _tickets.Aggregate()
                .Group(t => t.Employee.Department.Name, g => new { Department = g.Key, Count = g.Count() })
                .ToList();

            var dict = new Dictionary<DepartmentType, int>();
            foreach (var r in results)
            {
                if (!string.IsNullOrWhiteSpace(r.Department) &&
                    Enum.TryParse<DepartmentType>(r.Department, true, out var dept))
                {
                    dict[dept] = r.Count;
                }
            }
            return dict;
        }

        public Dictionary<TicketStatus, int> GetTicketCountsByStatusForEmployee(string employeeId)
        {
            var filter = Builders<Ticket>.Filter.Eq(t => t.Employee.EmployeeId, employeeId);
            var results = _tickets.Aggregate()
                .Match(filter)
                .Group(t => t.Status, g => new { Status = g.Key, Count = g.Count() })
                .ToList();

            return results.ToDictionary(r => r.Status, r => r.Count);
        }

        public string GetNextTicketId()
        {
            var counterFilter = Builders<Counter>.Filter.Eq(c => c.Id, TicketCounterId);
            var update = Builders<Counter>.Update.Inc(c => c.SequenceValue, 1);
            var options = new FindOneAndUpdateOptions<Counter>
            {
                ReturnDocument = ReturnDocument.After
            };

            var counter = _counters.FindOneAndUpdate(counterFilter, update, options);
            if (counter == null)
            {
                var seedValue = DetermineTicketSeedValue() + 1;
                var newCounter = new Counter
                {
                    Id = TicketCounterId,
                    SequenceValue = seedValue
                };
                _counters.InsertOne(newCounter);
                return FormatTicketId(seedValue);
            }
            return FormatTicketId(counter.SequenceValue);
        }

        public int GetNextServiceDeskAgentIndex(int agentCount)
        {
            var counterFilter = Builders<Counter>.Filter.Eq(c => c.Id, ServiceDeskRotationCounterId);
            var update = Builders<Counter>.Update.Inc(c => c.SequenceValue, 1);
            var options = new FindOneAndUpdateOptions<Counter>
            {
                ReturnDocument = ReturnDocument.After,
                IsUpsert = true
            };

            var counter = _counters.FindOneAndUpdate(counterFilter, update, options);
            return (int)(counter.SequenceValue % agentCount);
        }

        public void SetAssignedAgent(string ticketId, CommentAuthorEmbedded agent)
        {
            var filter = Builders<Ticket>.Filter.Eq(t => t.Id, ticketId);
            var update = Builders<Ticket>.Update.Set(t => t.AssignedTo, agent);
            _tickets.UpdateOne(filter, update);
        }

        private long DetermineTicketSeedValue()
        {
            const long defaultSeed = 1000;
            var ticketIds = _tickets.Find(FilterDefinition<Ticket>.Empty)
                .Project(t => t.TicketId)
                .ToList();

            if (ticketIds.Count == 0)
                return defaultSeed;

            var maxValue = defaultSeed;
            foreach (var ticketId in ticketIds)
            {
                if (TryParseTicketNumber(ticketId, out var parsed) && parsed > maxValue)
                    maxValue = parsed;
            }

            return maxValue;
        }

        private static bool TryParseTicketNumber(string ticketId, out long number)
        {
            number = 0;
            if (string.IsNullOrWhiteSpace(ticketId))
                return false;

            var numericPart = new string(ticketId.Where(char.IsDigit).ToArray());
            return long.TryParse(numericPart, out number);
        }

        private static string FormatTicketId(long number) => $"INC-{number:D4}";

        public void EnsureIndexes()
        {
            var models = new[]
            {
                new CreateIndexModel<Ticket>(
                    Builders<Ticket>.IndexKeys.Ascending(t => t.TicketId),
                    new CreateIndexOptions { Unique = true, Name = "ux_ticketId" }),
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

            _tickets.Indexes.CreateMany(models);
        }
    }
}

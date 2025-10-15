using System;
using System.Collections.Generic;
using System.Linq;
using IncidentManagementsSystemNOSQL.Models;
using MongoDB.Driver;

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
            try
            {
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

        public Dictionary<string, int> GetTicketCountsByStatusForEmployee(string employeeId)
        {
            try
            {
                var filter = Builders<Ticket>.Filter.Eq(t => t.Employee.EmployeeId, employeeId);

                var results = _tickets.Aggregate()
                    .Match(filter)
                    .Group(t => t.Status, g => new { Status = g.Key, Count = g.Count() })
                    .ToList();

                return results.ToDictionary(r => r.Status, r => r.Count);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while aggregating tickets by status for employee '{employeeId}'", ex);
            }
        }

        public string GetNextTicketId()
        {
            try
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

                    try
                    {
                        var newCounter = new Counter
                        {
                            Id = TicketCounterId,
                            SequenceValue = seedValue
                        };

                        _counters.InsertOne(newCounter);
                        return FormatTicketId(seedValue);
                    }
                    catch (MongoWriteException writeEx) when (writeEx.WriteError?.Category == ServerErrorCategory.DuplicateKey)
                    {
                        counter = _counters.FindOneAndUpdate(counterFilter, update, options);
                        if (counter != null)
                        {
                            return FormatTicketId(counter.SequenceValue);
                        }
                    }

                    throw new InvalidOperationException("Unable to initialize the ticket ID counter.");
                }

                return FormatTicketId(counter.SequenceValue);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while generating the next ticket ID", ex);
            }
        }

        public int GetNextServiceDeskAgentIndex(int agentCount)
        {
            if (agentCount <= 0)
            {
                throw new ArgumentException("Agent count must be greater than zero.", nameof(agentCount));
            }

            try
            {
                var counterFilter = Builders<Counter>.Filter.Eq(c => c.Id, ServiceDeskRotationCounterId);
                var update = Builders<Counter>.Update.Inc(c => c.SequenceValue, 1);
                var options = new FindOneAndUpdateOptions<Counter>
                {
                    ReturnDocument = ReturnDocument.After,
                    IsUpsert = true
                };

                var counter = _counters.FindOneAndUpdate(counterFilter, update, options);
                if (counter == null)
                {
                    throw new InvalidOperationException("Unable to advance service desk rotation counter.");
                }

                return (int)(counter.SequenceValue % agentCount);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while generating the next service desk agent index", ex);
            }
        }

        public void SetAssignedAgent(string ticketId, CommentAuthorEmbedded agent)
        {
            if (string.IsNullOrWhiteSpace(ticketId))
            {
                throw new ArgumentException("Ticket id is required", nameof(ticketId));
            }

            try
            {
                var filter = Builders<Ticket>.Filter.Eq(t => t.Id, ticketId);
                var update = Builders<Ticket>.Update.Set(t => t.AssignedAgents, new List<CommentAuthorEmbedded> { agent });
                _tickets.UpdateOne(filter, update);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while assigning service desk agent to ticket '{ticketId}'", ex);
            }
        }

        private long DetermineTicketSeedValue()
        {
            const long defaultSeed = 1000;

            try
            {
                var ticketIds = _tickets.Find(FilterDefinition<Ticket>.Empty)
                    .Project(t => t.TicketId)
                    .ToList();

                if (ticketIds.Count == 0)
                {
                    return defaultSeed;
                }

                var maxValue = defaultSeed;

                foreach (var ticketId in ticketIds)
                {
                    if (TryParseTicketNumber(ticketId, out var parsed) && parsed > maxValue)
                    {
                        maxValue = parsed;
                    }
                }

                return maxValue;
            }
            catch
            {
                return defaultSeed;
            }
        }

        private static bool TryParseTicketNumber(string ticketId, out long number)
        {
            number = 0;

            if (string.IsNullOrWhiteSpace(ticketId))
            {
                return false;
            }

            var numericPart = new string(ticketId.Where(char.IsDigit).ToArray());
            return long.TryParse(numericPart, out number);
        }

        private static string FormatTicketId(long number) => $"INC-{number:D4}";

        // Makes query faster and ensures uniqueness
        public void EnsureIndexes()
        {
            try
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

                _tickets.Indexes.CreateManyAsync(models).Wait();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while ensuring indexes for tickets collection", ex);
            }
        }
    }
}

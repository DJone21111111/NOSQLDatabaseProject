using IncidentManagementsSystemNOSQL.Models;
using MongoDB.Driver;

namespace IncidentManagementsSystemNOSQL.Repositories
{
    public class TicketRepository : ITicketRepository
    {
        private readonly IMongoCollection<Ticket> _tickets;

        public TicketRepository(IMongoDatabase db)
        {
            _tickets = db.GetCollection<Ticket>("tickets");
        }


        public async Task<Ticket?> GetById(string id)
        {
            try
            {
                return await _tickets.Find(t => t.Id == id).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while retrieving ticket with id '{id}'", ex);
            }
        }

        public async Task<List<Ticket>> GetByUserId(string userId)
        {
            try
            {
                return await _tickets.Find(t => t.Employee.EmployeeId == userId).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while retrieving tickets for user '{userId}'", ex);
            }
        }

        public async Task<List<Ticket>> GetAll()
        {
            try
            {
                return await _tickets.Find(FilterDefinition<Ticket>.Empty)
                                     .SortByDescending(t => t.DateCreated)
                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while retrieving all tickets", ex);
            }
        }

        public async Task<List<Ticket>> GetByStatus(Enums.TicketStatus status)
        {
            try
            {
                return await _tickets.Find(t => t.Status == status).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while retrieving tickets with status '{status}'", ex);
            }
        }


        public async Task AddTicket(Ticket ticket)
        {
            try
            {
                await _tickets.InsertOneAsync(ticket);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while adding new ticket", ex);
            }
        }

        public async Task UpdateTicket(Ticket ticket)
        {
            try
            {
                await _tickets.ReplaceOneAsync(t => t.Id == ticket.Id, ticket);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while updating ticket '{ticket.Id}'", ex);
            }
        }

        public async Task DeleteById(string id)
        {
            try
            {
                await _tickets.DeleteOneAsync(t => t.Id == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while deleting ticket with id '{id}'", ex);
            }
        }

        public async Task<Dictionary<Enums.TicketStatus, int>> GetTicketCountsByStatus()
        {
            try
            {
                var results = await _tickets.Aggregate()
                    .Group(t => t.Status, g => new { Status = g.Key, Count = g.Count() })
                    .ToListAsync();

                return results.ToDictionary(r => r.Status, r => r.Count);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while aggregating tickets by status", ex);
            }
        }

        public async Task<Dictionary<string, int>> GetTicketCountsByDepartment()
        {
            try
            {
                var results = await _tickets.Aggregate()
                    .Group(t => t.Employee.Department.Name, g => new { Department = g.Key, Count = g.Count() })
                    .ToListAsync();

                return results.ToDictionary(r => r.Department, r => r.Count);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while aggregating tickets by department", ex);
            }
        }


        public async Task EnsureIndexesAsync(CancellationToken ct = default)
        {
            try
            {
                var models = new[]
                {
                    new CreateIndexModel<Ticket>(
                        Builders<Ticket>.IndexKeys.Ascending(t => t.Status),
                        new CreateIndexOptions { Name = "ix_status" }),

                    new CreateIndexModel<Ticket>(
                        Builders<Ticket>.IndexKeys.Ascending("CreatedBy.Id"),
                        new CreateIndexOptions { Name = "ix_createdById" }),

                    new CreateIndexModel<Ticket>(
                        Builders<Ticket>.IndexKeys.Descending(t => t.DateCreated),
                        new CreateIndexOptions { Name = "ix_createdAt" })
                };

                await _tickets.Indexes.CreateManyAsync(models, ct);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while ensuring indexes for tickets collection", ex);
            }
        }
    }
}

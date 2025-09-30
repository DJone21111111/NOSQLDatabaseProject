using IncidentManagementsSystemNOSQL.Models;
using MongoDB.Driver;

namespace IncidentManagementsSystemNOSQL.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _users;

        public UserRepository(IMongoDatabase db)
        {
            _users = db.GetCollection<User>("users");
        }

        public async Task<User?> GetByUsername(string username)
        {
            try
            {
                return await _users.Find(u => u.UserName == username).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while retrieving user with username '{username}'", ex);
            }
        }

        public async Task<User?> GetByEmployeeId(string employeeId)
        {
            try
            {
                return await _users.Find(u => u.EmployeeId == employeeId).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while retrieving user with employeeId '{employeeId}'", ex);
            }
        }

        public async Task<User> GetById(string id)
        {
            try
            {
                var user = await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
                if (user is null)
                    throw new KeyNotFoundException($"User with id '{id}' not found.");
                return user;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while retrieving user with id '{id}'", ex);
            }
        }

        public async Task<List<User>> GetAll()
        {
            try
            {
                return await _users.Find(FilterDefinition<User>.Empty)
                                   .SortBy(u => u.EmployeeId)
                                   .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while retrieving all users", ex);
            }
        }


        public async Task AddUser(User user)
        {
            try
            {
                await _users.InsertOneAsync(user);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while adding user '{user.EmployeeId}'", ex);
            }
        }

        public async Task UpdateUser(User user)
        {
            try
            {
                await _users.ReplaceOneAsync(u => u.Id == user.Id, user);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while updating user '{user.EmployeeId}'", ex);
            }
        }

        public async Task DeleteById(string id)
        {
            try
            {
                await _users.DeleteOneAsync(u => u.Id == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while deleting user with id '{id}'", ex);
            }
        }

        // Makes query faster and ensures uniqueness
        public async Task EnsureIndexes(CancellationToken ct = default)
        {
            try
            {
                var models = new[]
                {
                    new CreateIndexModel<User>(
                        Builders<User>.IndexKeys.Ascending(u => u.EmployeeId),
                        new CreateIndexOptions { Unique = true, Name = "ux_employeeId" }),

                    new CreateIndexModel<User>(
                        Builders<User>.IndexKeys.Ascending(u => u.Email),
                        new CreateIndexOptions { Unique = true, Name = "ux_email" }),

                    new CreateIndexModel<User>(
                        Builders<User>.IndexKeys.Ascending(u => u.UserName),
                        new CreateIndexOptions { Unique = true, Name = "ux_username" }),
                };

                await _users.Indexes.CreateManyAsync(models, ct);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while ensuring indexes for users collection", ex);
            }
        }
    }
}

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

        public User? GetByUsername(string username)
        {
            try
            {
                return _users.Find(u => u.UserName == username).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while retrieving user with username '{username}'", ex);
            }
        }

        public User? GetByEmployeeId(string employeeId)
        {
            try
            {
                return _users.Find(u => u.EmployeeId == employeeId).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while retrieving user with employeeId '{employeeId}'", ex);
            }
        }

        public User? GetById(string id)
        {
            try
            {
                return _users.Find(u => u.Id == id).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while retrieving user with id '{id}'", ex);
            }
        }

        public List<User> GetAll()
        {
            try
            {
                return _users.Find(FilterDefinition<User>.Empty)
                                       .SortBy(u => u.EmployeeId)
                                       .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while retrieving all users", ex);
            }
        }

        public void SetPasswordHash(string id, string newPasswordHash)
        {
            try
            {
                var filter = Builders<User>.Filter.Eq(u => u.Id, id);
                var update = Builders<User>.Update
                    .Set(u => u.PasswordHash, newPasswordHash)
                    .Set(u => u.UpdatedAt, DateTime.UtcNow); 

                _users.UpdateOne(filter, update);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while setting password hash for user '{id}'", ex);
            }
        }

        public void AddUser(User user)
        {
            try
            {
                _users.InsertOne(user);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while adding user '{user.EmployeeId}'", ex);
            }
        }

        public void UpdateUser(User user)
        {
            try
            {
                _users.ReplaceOne(u => u.Id == user.Id, user);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while updating user '{user.EmployeeId}'", ex);
            }
        }

        public void DeleteById(string id)
        {
            try
            {
                _users.DeleteOne(u => u.Id == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while deleting user with id '{id}'", ex);
            }
        }

        public void EnsureIndexes()
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

                _users.Indexes.CreateManyAsync(models).Wait();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while ensuring indexes for users collection", ex);
            }
        }
    }
}

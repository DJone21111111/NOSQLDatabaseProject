using IncidentManagementsSystemNOSQL.Models;
using MongoDB.Driver;
using System.Linq;

namespace IncidentManagementsSystemNOSQL.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Counter> _counters;
        private const string EmployeeCounterId = "employee-sequence";

        public UserRepository(IMongoDatabase db)
        {
            _users = db.GetCollection<User>("users");
            _counters = db.GetCollection<Counter>("counters");
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

        public string GetNextEmployeeId()
        {
            try
            {
                var counterFilter = Builders<Counter>.Filter.Eq(c => c.Id, EmployeeCounterId);
                var update = Builders<Counter>.Update.Inc(c => c.SequenceValue, 1);
                var options = new FindOneAndUpdateOptions<Counter>
                {
                    ReturnDocument = ReturnDocument.After
                };

                var counter = _counters.FindOneAndUpdate(counterFilter, update, options);

                if (counter == null)
                {
                    var seedValue = DetermineSeedValue() + 1;

                    try
                    {
                        var newCounter = new Counter
                        {
                            Id = EmployeeCounterId,
                            SequenceValue = seedValue
                        };

                        _counters.InsertOne(newCounter);
                        return FormatEmployeeId(seedValue);
                    }
                    catch (MongoWriteException writeEx) when (writeEx.WriteError?.Category == ServerErrorCategory.DuplicateKey)
                    {
                        counter = _counters.FindOneAndUpdate(counterFilter, update, options);
                        if (counter != null)
                        {
                            return FormatEmployeeId(counter.SequenceValue);
                        }
                    }

                    throw new InvalidOperationException("Unable to initialize the employee ID counter.");
                }

                return FormatEmployeeId(counter.SequenceValue);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while generating the next employee ID", ex);
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

        private long DetermineSeedValue()
        {
            const long defaultSeed = 1000;

            try
            {
                var employeeIds = _users.Find(FilterDefinition<User>.Empty)
                    .Project(u => u.EmployeeId)
                    .ToList();

                if (employeeIds.Count == 0)
                {
                    return defaultSeed;
                }

                var maxValue = defaultSeed;

                foreach (var employeeId in employeeIds)
                {
                    if (TryParseEmployeeNumber(employeeId, out var parsed) && parsed > maxValue)
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

        private static bool TryParseEmployeeNumber(string employeeId, out long number)
        {
            number = 0;

            if (string.IsNullOrWhiteSpace(employeeId))
            {
                return false;
            }

            var numericPart = new string(employeeId.Where(char.IsDigit).ToArray());
            return long.TryParse(numericPart, out number);
        }

        private static string FormatEmployeeId(long number) => $"EMP-{number:D4}";
    }
}

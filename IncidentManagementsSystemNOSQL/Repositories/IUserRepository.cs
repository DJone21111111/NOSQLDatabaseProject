using IncidentManagementsSystemNOSQL.Models;

namespace IncidentManagementsSystemNOSQL.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByUsername(string username);
        Task<User?> GetByEmployeeId(string employeeId);

        Task<User> GetById(string id);
        Task<List<User>> GetAll();

        Task AddUser(User user);
        Task UpdateUser(User user);
        Task DeleteById(string id);

        // Makes query faster and ensures uniqueness
        Task EnsureIndexes(CancellationToken ct = default);
    }
}

using IncidentManagementsSystemNOSQL.Models;
namespace IncidentManagementsSystemNOSQL.Service
{
    public interface IUserService
    {
        Task<List<User>> GetAllUsers();
        Task<User?> GetUserById(string id);
        Task<User?> GetUserByEmployeeId(string employeeId);
        Task<User?> GetUserByUsername(string username);

        Task AddUser (User user);
        Task UpdateUser(string id, User updatedUser);
        Task DeleteUser(string id);
    }
}

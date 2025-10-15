using IncidentManagementsSystemNOSQL.Models;

namespace IncidentManagementsSystemNOSQL.Repositories
{
    public interface IUserRepository
    {
        User? GetByUsername(string username);
        User? GetByEmployeeId(string employeeId);
        User? GetById(string id);
        List<User> GetAll();
    List<User> GetServiceDeskAgents();

        void SetPasswordHash(string id, string newPasswordHash);

        void AddUser(User user);
        void UpdateUser(User user);
        void DeleteById(string id);
        string GetNextEmployeeId();
        void EnsureIndexes();
    }
}

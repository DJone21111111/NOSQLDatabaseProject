using IncidentManagementsSystemNOSQL.Models;

namespace IncidentManagementsSystemNOSQL.Service
{
    public interface IUserService
    {
        List<User> GetAllUsers();
        User? GetUserById(string id);
        User? GetUserByEmployeeId(string employeeId);
        User? GetUserByUsername(string username);
        List<User> GetServiceDeskAgents();

        void AddUser(User user);
        void UpdateUser(string id, User updatedUser);
        void DeleteUser(string id);
        string GetNextEmployeeId();
    }
}

using IncidentManagementsSystemNOSQL.Models;
using IncidentManagementsSystemNOSQL.Repositories;

namespace IncidentManagementsSystemNOSQL.Service
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // Get all users
        public async Task<List<User>> GetAllUsers()
        {
            try
            {
                return await _userRepository.GetAll();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving users.", ex);
            }
        }

        // Get user by ID
        public async Task<User?> GetUserById(string id)
        {
            try
            {
                return await _userRepository.GetById(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while retrieving user with ID {id}.", ex);
            }
        }

        // Get user by employeeId
        public async Task<User?> GetUserByEmployeeId(string employeeId)
        {
            try
            {
                return await _userRepository.GetByEmployeeId(employeeId);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while retrieving user with employeeId {employeeId}.", ex);
            }
        }

        // Get user by username
        public async Task<User?> GetUserByUsername(string username)
        {
            try
            {
                return await _userRepository.GetByUsername(username);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while retrieving user with username {username}.", ex);
            }
        }

        // Add a new user
        public async Task AddUserAsync(User user)
        {
            try
            {
                user.CreatedAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.AddUser(user);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while adding a new user.", ex);
            }
        }

        // Update existing user
        public async Task UpdateUser(User user)
        {
            try
            {
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateUser(user);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while updating user {user.EmployeeId}.", ex);
            }
        }

        // Delete user
        public async Task DeleteUser(string id)
        {
            try
            {
                await _userRepository.DeleteById(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while deleting user {id}.", ex);
            }
        }
    }
}

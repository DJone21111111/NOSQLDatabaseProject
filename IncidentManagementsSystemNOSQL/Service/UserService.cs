using IncidentManagementsSystemNOSQL.Models;
using IncidentManagementsSystemNOSQL.Repositories;

namespace IncidentManagementsSystemNOSQL.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public List<User> GetAllUsers()
        {
            try
            {
                return _userRepository.GetAll();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving users.", ex);
            }
        }

        public User? GetUserById(string id)
        {
            try
            {
                return _userRepository.GetById(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while retrieving user with ID {id}.", ex);
            }
        }

        public User? GetUserByEmployeeId(string employeeId)
        {
            try
            {
                return _userRepository.GetByEmployeeId(employeeId);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while retrieving user with employeeId {employeeId}.", ex);
            }
        }

        public User? GetUserByUsername(string username)
        {
            try
            {
                return _userRepository.GetByUsername(username);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while retrieving user with username {username}.", ex);
            }
        }

        public void AddUser(User user)
        {
            try
            {
                user.CreatedAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;

                _userRepository.AddUser(user);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while adding a new user.", ex);
            }
        }

        public void UpdateUser(string id, User updatedUser)
        {
            try
            {
                updatedUser.Id = id;
                updatedUser.UpdatedAt = DateTime.UtcNow;

                _userRepository.UpdateUser(updatedUser);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while updating user {id}.", ex);
            }
        }

        public void DeleteUser(string id)
        {
            try
            {
                _userRepository.DeleteById(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while deleting user {id}.", ex);
            }
        }
    }
}

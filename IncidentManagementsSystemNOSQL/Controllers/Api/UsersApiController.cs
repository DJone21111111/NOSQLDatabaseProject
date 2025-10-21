using Microsoft.AspNetCore.Mvc;
using IncidentManagementsSystemNOSQL.Models;
using IncidentManagementsSystemNOSQL.Models.Api;
using IncidentManagementsSystemNOSQL.Service;
using static IncidentManagementsSystemNOSQL.Models.Enums;

namespace IncidentManagementsSystemNOSQL.Controllers.Api
{
    [ApiController]
    [Route("api/v1/users")]
    public class UsersApiController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILogger<UsersApiController> _logger;

        public UsersApiController(IUserService userService, IPasswordHasher passwordHasher, ILogger<UsersApiController> logger)
        {
            _userService = userService;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<IEnumerable<UserDto>> GetAll()
        {
            try
            {
                List<User> users = _userService.GetAllUsers();
                List<UserDto> response = new List<UserDto>();
                foreach (User user in users)
                {
                    response.Add(MapToUserDto(user));
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve users");
                return Problem(detail: "An unexpected error occurred while listing users.", statusCode: 500);
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<UserDto> GetById(string id)
        {
            try
            {
                User? user = _userService.GetUserById(id);
                if (user == null)
                {
                    return NotFound();
                }

                return Ok(MapToUserDto(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve user {UserId}", id);
                return Problem(detail: "An unexpected error occurred while retrieving the user.", statusCode: 500);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<UserDto> Create([FromBody] UserCreateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            try
            {
                if (!Enum.TryParse<UserRole>(request.Role, true, out var parsedRole))
                {
                    ModelState.AddModelError(nameof(request.Role), "Invalid role.");
                    return ValidationProblem(ModelState);
                }

                string employeeId = _userService.GetNextEmployeeId();

                var department = new DepartmentEmbedded
                {
                    DepartmentId = null,
                    Name = request.DepartmentName,
                    Description = request.DepartmentDescription
                };

                var user = new User
                {
                    EmployeeId = employeeId,
                    Name = request.Name,
                    Email = request.Email,
                    Role = parsedRole,
                    Department = department,
                    UserName = request.UserName,
                    IsActive = request.IsActive,
                    MustChangePassword = request.MustChangePassword,
                    PasswordHash = _passwordHasher.HashPassword(request.Password)
                };

                _userService.AddUser(user);

                var dto = MapToUserDto(user);
                return CreatedAtAction(nameof(GetById), new { id = user.Id }, dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create user {UserName}", request.UserName);
                return Problem(detail: "An unexpected error occurred while creating the user.", statusCode: 500);
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<UserDto> Update(string id, [FromBody] UserUpdateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            try
            {
                User? existingUser = _userService.GetUserById(id);
                if (existingUser == null)
                {
                    return NotFound();
                }

                if (!Enum.TryParse<UserRole>(request.Role, true, out var parsedRole))
                {
                    ModelState.AddModelError(nameof(request.Role), "Invalid role.");
                    return ValidationProblem(ModelState);
                }

                existingUser.Name = request.Name;
                existingUser.Email = request.Email;
                existingUser.Role = parsedRole;
                existingUser.UserName = request.UserName;
                existingUser.IsActive = request.IsActive;
                existingUser.MustChangePassword = request.MustChangePassword;
                existingUser.Department = new DepartmentEmbedded
                {
                    DepartmentId = existingUser.Department?.DepartmentId,
                    Name = request.DepartmentName,
                    Description = request.DepartmentDescription
                };

                if (!string.IsNullOrWhiteSpace(request.Password))
                {
                    existingUser.PasswordHash = _passwordHasher.HashPassword(request.Password);
                }

                _userService.UpdateUser(id, existingUser);
                var dto = MapToUserDto(existingUser);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user {UserId}", id);
                return Problem(detail: "An unexpected error occurred while updating the user.", statusCode: 500);
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Delete(string id)
        {
            try
            {
                User? existingUser = _userService.GetUserById(id);
                if (existingUser == null)
                {
                    return NotFound();
                }

                _userService.DeleteUser(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete user {UserId}", id);
                return Problem(detail: "An unexpected error occurred while deleting the user.", statusCode: 500);
            }
        }

        private static UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id ?? string.Empty,
                EmployeeId = user.EmployeeId ?? string.Empty,
                Name = user.Name ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Role = user.Role.ToString(),
                DepartmentName = user.Department?.Name ?? string.Empty,
                DepartmentDescription = user.Department?.Description ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                IsActive = user.IsActive,
                MustChangePassword = user.MustChangePassword,
                CreatedAtUtc = user.CreatedAt,
                UpdatedAtUtc = user.UpdatedAt
            };
        }
    }
}

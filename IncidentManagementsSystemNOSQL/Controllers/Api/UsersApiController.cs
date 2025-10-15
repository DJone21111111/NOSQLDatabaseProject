using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using IncidentManagementsSystemNOSQL.Models;
using IncidentManagementsSystemNOSQL.Models.Api;
using IncidentManagementsSystemNOSQL.Service;

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

    /// <summary>
    /// Retrieves the current user directory.
    /// </summary>
    /// <returns>A collection of <see cref="UserDto"/> entries.</returns>
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

    /// <summary>
    /// Retrieves a user by MongoDB identifier.
    /// </summary>
    /// <param name="id">The MongoDB document identifier of the user.</param>
    /// <returns>The matching <see cref="UserDto"/> when found.</returns>
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

    /// <summary>
    /// Creates a new user record and assigns a generated employee identifier.
    /// </summary>
    /// <param name="request">The details required to create a user.</param>
    /// <returns>The created <see cref="UserDto"/>.</returns>
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
                string employeeId = _userService.GetNextEmployeeId();

                DepartmentEmbedded department = new DepartmentEmbedded
                {
                    DepartmentId = null,
                    Name = request.DepartmentName,
                    Description = request.DepartmentDescription
                };

                User user = new User
                {
                    EmployeeId = employeeId,
                    Name = request.Name,
                    Email = request.Email,
                    Role = request.Role,
                    Department = department,
                    UserName = request.UserName,
                    IsActive = request.IsActive,
                    MustChangePassword = request.MustChangePassword,
                    PasswordHash = _passwordHasher.HashPassword(request.Password)
                };

                _userService.AddUser(user);

                UserDto dto = MapToUserDto(user);
                return CreatedAtAction(nameof(GetById), new { id = user.Id }, dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create user {UserName}", request.UserName);
                return Problem(detail: "An unexpected error occurred while creating the user.", statusCode: 500);
            }
        }

    /// <summary>
    /// Updates an existing user record.
    /// </summary>
    /// <param name="id">The MongoDB document identifier of the user.</param>
    /// <param name="request">The fields to update on the user.</param>
    /// <returns>The updated <see cref="UserDto"/>.</returns>
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

                existingUser.Name = request.Name;
                existingUser.Email = request.Email;
                existingUser.Role = request.Role;
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
                UserDto dto = MapToUserDto(existingUser);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user {UserId}", id);
                return Problem(detail: "An unexpected error occurred while updating the user.", statusCode: 500);
            }
        }

    /// <summary>
    /// Deletes a user from the directory.
    /// </summary>
    /// <param name="id">The MongoDB document identifier of the user.</param>
    /// <returns>No content when the user is successfully removed.</returns>
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
            UserDto dto = new UserDto
            {
                Id = user.Id ?? string.Empty,
                EmployeeId = user.EmployeeId ?? string.Empty,
                Name = user.Name ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Role = user.Role ?? string.Empty,
                DepartmentName = user.Department?.Name ?? string.Empty,
                DepartmentDescription = user.Department?.Description ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                IsActive = user.IsActive,
                MustChangePassword = user.MustChangePassword,
                CreatedAtUtc = user.CreatedAt,
                UpdatedAtUtc = user.UpdatedAt
            };

            return dto;
        }
    }
}

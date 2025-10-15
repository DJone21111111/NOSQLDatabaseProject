using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using IncidentManagementsSystemNOSQL.Models;
using IncidentManagementsSystemNOSQL.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace IncidentManagementsSystemNOSQL.Controllers
{
    public class UserController : Controller
    {
    private readonly IUserService _userService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<UserController> _logger;
    private static readonly string[] _allRoleOptions = new[] { "employee", "service_desk", "manager", "admin" };
    private static readonly string[] _createRoleOptions = new[] { "employee", "service_desk" };

        public UserController(IUserService userService, IPasswordHasher passwordHasher, ILogger<UserController> logger)
        {
            _userService = userService;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public IActionResult Index()
        {
            try
            {
                List<User> users = _userService.GetAllUsers();
                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching users");
                return StatusCode(500, "An error occurred while retrieving users.");
            }
        }

        public IActionResult Details(string id)
        {
            try
            {
                User? user = _userService.GetUserById(id);
                if (user == null)
                {
                    return NotFound();
                }
                UserFormViewModel model = MapToViewModel(user);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user {UserId}", id);
                return StatusCode(500, "An error occurred while retrieving the user.");
            }
        }

        public IActionResult Create()
        {
            try
            {
                UserFormViewModel model = new UserFormViewModel
                {
                    IsActive = true,
                    MustChangePassword = true,
                    Role = "employee"
                };
                PopulateSelections(model.Role, _createRoleOptions);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering user creation form");
                return StatusCode(500, "An error occurred while preparing the user creation form.");
            }
        }

    [HttpPost]
    //[ValidateAntiForgeryToken] // TODO: Re-enable once anti-forgery issues are resolved
        public IActionResult Create(UserFormViewModel? model)
        {
            if (model == null)
            {
                _logger.LogWarning("Create POST received a null model");
                ModelState.AddModelError(string.Empty, "We did not receive any user details. Please try again.");
                UserFormViewModel emptyModel = new UserFormViewModel
                {
                    IsActive = true,
                    MustChangePassword = true,
                    Role = "employee"
                };
                PopulateSelections(emptyModel.Role, _createRoleOptions);
                return View(emptyModel);
            }

            UserFormViewModel requestModel = model;

            if (string.IsNullOrWhiteSpace(requestModel.Password))
            {
                ModelState.AddModelError(nameof(requestModel.Password), "Password is required.");
            }

            if (!ModelState.IsValid)
            {
                PopulateSelections(requestModel.Role, _createRoleOptions);
                return View(requestModel);
            }

            try
            {
                string employeeId = _userService.GetNextEmployeeId();
                requestModel.EmployeeId = employeeId;

                User user = MapToUser(requestModel);
                user.PasswordHash = _passwordHasher.HashPassword(requestModel.Password!);
                user.CreatedAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;

                _userService.AddUser(user);
                TempData["SuccessMessage"] = $"User {user.EmployeeId} created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user with username {Username}", requestModel.UserName);
                ModelState.AddModelError("", "Unable to create user. Please try again.");
                PopulateSelections(requestModel.Role, _createRoleOptions);
                return View(requestModel);
            }
        }

        public IActionResult Edit(string id)
        {
            try
            {
                _logger.LogInformation("Edit GET requested for user {UserId}", id);
                User? user = _userService.GetUserById(id);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found during edit GET", id);
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }

                UserFormViewModel model = MapToViewModel(user);
                _logger.LogInformation("Loaded user {EmployeeId} for edit", model.EmployeeId);
                PopulateSelections(model.Role, _createRoleOptions);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {UserId} for edit", id);
                TempData["ErrorMessage"] = "An error occurred while retrieving the user.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]  // Temporarily disabled for debugging
        public IActionResult Edit(string? id, UserFormViewModel? model)
        {
            _logger.LogInformation("Edit POST called. Route id: {RouteId}, Model.Id: {ModelId}", id ?? "NULL", model?.Id ?? "NULL");
            
            if (model == null)
            {
                _logger.LogWarning("Edit POST received a null model");
                TempData["ErrorMessage"] = "Invalid form data received.";
                return RedirectToAction(nameof(Index));
            }

            // Use model.Id if route id is missing (shouldn't happen but let's be safe)
            string? userId = id ?? model.Id;
            
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Edit POST missing both route id and model.Id");
                TempData["ErrorMessage"] = "User ID is missing.";
                return RedirectToAction(nameof(Index));
            }

            if (!string.IsNullOrEmpty(id) && id != model.Id)
            {
                _logger.LogWarning("ID mismatch during edit POST. Route id={RouteId}, model id={ModelId}", id, model.Id);
                TempData["ErrorMessage"] = "User ID mismatch.";
                return RedirectToAction(nameof(Index));
            }

            // Remove password validation if fields are empty (user doesn't want to change password)
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.Remove(nameof(UserFormViewModel.Password));
                ModelState.Remove(nameof(UserFormViewModel.ConfirmPassword));
                model.Password = null;
                model.ConfirmPassword = null;
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Edit POST model state invalid");
                foreach (string key in ModelState.Keys)
                {
                    Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateEntry? state = ModelState[key];
                    if (state?.Errors.Count > 0)
                    {
                        foreach (Microsoft.AspNetCore.Mvc.ModelBinding.ModelError error in state.Errors)
                        {
                            _logger.LogWarning("Validation error on {Key}: {Message}", key, error.ErrorMessage);
                        }
                    }
                }
                PopulateSelections(model.Role, _createRoleOptions);
                return View(model);
            }

            try
            {
                _logger.LogInformation("Fetching existing user {UserId} from database", userId);
                User? existingUser = _userService.GetUserById(userId);
                if (existingUser == null)
                {
                    _logger.LogWarning("User {UserId} not found in database during edit POST", userId);
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation("Updating user {EmployeeId}", existingUser.EmployeeId);
                
                // Update only the editable fields from the form
                if (!string.IsNullOrWhiteSpace(model.EmployeeId))
                {
                    existingUser.EmployeeId = model.EmployeeId;
                }
                existingUser.Name = model.Name;
                existingUser.Email = model.Email;
                existingUser.Role = model.Role;
                existingUser.UserName = model.UserName;
                existingUser.IsActive = model.IsActive;
                existingUser.MustChangePassword = model.MustChangePassword;
                
                // Preserve existing department ID, update name and description
                string? currentDepartmentId = existingUser.Department?.DepartmentId;
                existingUser.Department = new DepartmentEmbedded
                {
                    DepartmentId = currentDepartmentId,
                    Name = model.DepartmentName,
                    Description = model.DepartmentDescription
                };

                // Only update password if provided
                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    _logger.LogInformation("Updating password hash for user {EmployeeId}", existingUser.EmployeeId);
                    existingUser.PasswordHash = _passwordHasher.HashPassword(model.Password);
                }
                // Otherwise keep the existing PasswordHash (already in existingUser)

                // Keep CreatedAt as-is, update timestamp
                existingUser.UpdatedAt = DateTime.UtcNow;
                
                _userService.UpdateUser(userId, existingUser);
                
                _logger.LogInformation("User {EmployeeId} updated successfully", existingUser.EmployeeId);
                TempData["SuccessMessage"] = $"User {existingUser.EmployeeId} updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", userId);
                TempData["ErrorMessage"] = $"Unable to update user: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult Delete(string id)
        {
            try
            {
                User? user = _userService.GetUserById(id);
                if (user == null)
                {
                    return NotFound();
                }
                UserFormViewModel model = MapToViewModel(user);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {UserId} for deletion", id);
                return StatusCode(500, "An error occurred while retrieving the user.");
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            try
            {
                _userService.DeleteUser(id);
                TempData["SuccessMessage"] = "User removed successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                return StatusCode(500, "An error occurred while deleting the user.");
            }
        }

        private static User MapToUser(UserFormViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.EmployeeId))
            {
                throw new InvalidOperationException("Employee ID must be provided before mapping to a user entity.");
            }

            User user = new User
            {
                EmployeeId = model.EmployeeId,
                Name = model.Name,
                Email = model.Email,
                Role = model.Role,
                UserName = model.UserName,
                IsActive = model.IsActive,
                MustChangePassword = model.MustChangePassword,
                Department = new DepartmentEmbedded
                {
                    DepartmentId = null,
                    Name = model.DepartmentName,
                    Description = model.DepartmentDescription
                }
            };

            user.Id = string.IsNullOrWhiteSpace(model.Id)
                ? ObjectId.GenerateNewId().ToString()
                : model.Id;

            return user;
        }

        private static UserFormViewModel MapToViewModel(User user)
        {
            return new UserFormViewModel
            {
                Id = user.Id,
                EmployeeId = user.EmployeeId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                UserName = user.UserName,
                IsActive = user.IsActive,
                MustChangePassword = user.MustChangePassword,
                DepartmentName = user.Department?.Name ?? string.Empty,
                DepartmentDescription = user.Department?.Description ?? string.Empty,
                LastUpdatedDisplay = user.UpdatedAt.ToLocalTime().ToString("MMM dd, yyyy HH:mm")
            };
        }

        private void PopulateSelections(string? selectedRole, IEnumerable<string>? allowedRoles = null)
        {
            List<string> baseRoles = (allowedRoles ?? _allRoleOptions).ToList();

            if (!string.IsNullOrWhiteSpace(selectedRole) && !baseRoles.Any(r => string.Equals(r, selectedRole, StringComparison.OrdinalIgnoreCase)))
            {
                baseRoles.Insert(0, selectedRole);
            }

            ViewBag.RoleOptions = baseRoles
                .Select(r => new SelectListItem
                {
                    Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(r.Replace('_', ' ')),
                    Value = r,
                    Selected = string.Equals(r, selectedRole, StringComparison.OrdinalIgnoreCase),
                    Disabled = allowedRoles != null && !allowedRoles.Any(ar => string.Equals(ar, r, StringComparison.OrdinalIgnoreCase))
                })
                .ToList();
        }
    }
}

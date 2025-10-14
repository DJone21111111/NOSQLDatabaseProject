using System.Globalization;
using System.Linq;
using IncidentManagementsSystemNOSQL.Models;
using IncidentManagementsSystemNOSQL.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MongoDB.Bson;

namespace IncidentManagementsSystemNOSQL.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IPasswordHasher _passwordHasher;
        private static readonly string[] _roleOptions = new[] { "employee", "service_desk", "manager", "admin" };

        public UserController(IUserService userService, IPasswordHasher passwordHasher)
        {
            _userService = userService;
            _passwordHasher = passwordHasher;
        }

        public IActionResult Index()
        {
            try
            {
                var users = _userService.GetAllUsers();
                return View(users);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching users: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving users.");
            }
        }

        public IActionResult Details(string id)
        {
            try
            {
                var user = _userService.GetUserById(id);
                if (user == null) return NotFound();
                var model = MapToViewModel(user);
                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching user {id}: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving the user.");
            }
        }

        public IActionResult Create()
        {
            var model = new UserFormViewModel
            {
                IsActive = true,
                MustChangePassword = true,
                Role = "employee"
            };
            PopulateSelections(model.Role);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(UserFormViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError(nameof(model.Password), "Password is required.");
            }

            if (!ModelState.IsValid)
            {
                PopulateSelections(model.Role);
                return View(model);
            }

            try
            {
                var user = MapToUser(model);
                user.PasswordHash = _passwordHasher.HashPassword(model.Password!);
                user.CreatedAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;

                _userService.AddUser(user);
                TempData["SuccessMessage"] = $"User {user.EmployeeId} created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating user: {ex.Message}");
                ModelState.AddModelError("", "Unable to create user. Please try again.");
                PopulateSelections(model.Role);
                return View(model);
            }
        }

        public IActionResult Edit(string id)
        {
            try
            {
                Console.WriteLine($"Edit GET: id={id}");
                var user = _userService.GetUserById(id);
                if (user == null)
                {
                    Console.WriteLine($"ERROR: User {id} not found");
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }
                
                var model = MapToViewModel(user);
                Console.WriteLine($"Loaded user: {model.Name} ({model.EmployeeId}) with ID {model.Id}");
                PopulateSelections(model.Role);
                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving user {id} for edit: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while retrieving the user.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]  // Temporarily disabled for debugging
        public IActionResult Edit(string? id, UserFormViewModel model)
        {
            Console.WriteLine($"=== Edit POST Called ===");
            Console.WriteLine($"Route id parameter: {id ?? "NULL"}");
            Console.WriteLine($"Model.Id: {model?.Id ?? "NULL"}");
            Console.WriteLine($"Model is null? {model == null}");
            
            if (model == null)
            {
                Console.WriteLine("ERROR: Model is null - form binding failed");
                TempData["ErrorMessage"] = "Invalid form data received.";
                return RedirectToAction(nameof(Index));
            }

            // Use model.Id if route id is missing (shouldn't happen but let's be safe)
            var userId = id ?? model.Id;
            
            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("ERROR: Both route id and model.Id are null/empty");
                TempData["ErrorMessage"] = "User ID is missing.";
                return RedirectToAction(nameof(Index));
            }

            if (!string.IsNullOrEmpty(id) && id != model.Id)
            {
                Console.WriteLine($"ERROR: ID mismatch - route id={id}, model id={model.Id}");
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
                Console.WriteLine("ModelState is invalid:");
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    if (state?.Errors.Count > 0)
                    {
                        foreach (var error in state.Errors)
                        {
                            Console.WriteLine($"  {key}: {error.ErrorMessage}");
                        }
                    }
                }
                PopulateSelections(model.Role);
                return View(model);
            }

            try
            {
                Console.WriteLine($"Fetching existing user {userId} from database...");
                var existingUser = _userService.GetUserById(userId);
                if (existingUser == null)
                {
                    Console.WriteLine($"ERROR: User {userId} not found in database");
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }

                Console.WriteLine($"Updating user {existingUser.EmployeeId}...");
                
                // Update only the editable fields from the form
                existingUser.EmployeeId = model.EmployeeId;
                existingUser.Name = model.Name;
                existingUser.Email = model.Email;
                existingUser.Role = model.Role;
                existingUser.UserName = model.UserName;
                existingUser.IsActive = model.IsActive;
                existingUser.MustChangePassword = model.MustChangePassword;
                
                // Preserve existing department ID, update name and description
                var currentDepartmentId = existingUser.Department?.DepartmentId;
                existingUser.Department = new DepartmentEmbedded
                {
                    DepartmentId = currentDepartmentId,
                    Name = model.DepartmentName,
                    Description = model.DepartmentDescription
                };

                // Only update password if provided
                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    Console.WriteLine("Updating password hash...");
                    existingUser.PasswordHash = _passwordHasher.HashPassword(model.Password);
                }
                // Otherwise keep the existing PasswordHash (already in existingUser)

                // Keep CreatedAt as-is, update timestamp
                existingUser.UpdatedAt = DateTime.UtcNow;
                
                _userService.UpdateUser(userId, existingUser);
                
                Console.WriteLine($"✓ User {existingUser.EmployeeId} updated successfully");
                TempData["SuccessMessage"] = $"User {existingUser.EmployeeId} updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR updating user {userId}: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = $"Unable to update user: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult Delete(string id)
        {
            try
            {
                var user = _userService.GetUserById(id);
                if (user == null) return NotFound();
                var model = MapToViewModel(user);
                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving user {id} for deletion: {ex.Message}");
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
                Console.WriteLine($"Error deleting user {id}: {ex.Message}");
                return StatusCode(500, "An error occurred while deleting the user.");
            }
        }

        private static User MapToUser(UserFormViewModel model)
        {
            var user = new User
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

        private void PopulateSelections(string? selectedRole)
        {
            ViewBag.RoleOptions = _roleOptions
                .Select(r => new SelectListItem
                {
                    Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(r.Replace('_', ' ')),
                    Value = r,
                    Selected = string.Equals(r, selectedRole, StringComparison.OrdinalIgnoreCase)
                })
                .ToList();
        }
    }
}

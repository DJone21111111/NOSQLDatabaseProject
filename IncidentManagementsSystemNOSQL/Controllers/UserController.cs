using IncidentManagementsSystemNOSQL.Models;
using IncidentManagementsSystemNOSQL.Repositories;
using IncidentManagementsSystemNOSQL.Service;
using Microsoft.AspNetCore.Mvc;

namespace IncidentManagementsSystemNOSQL.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
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
                return View(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching user {id}: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving the user.");
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(User user)
        {
            if (!ModelState.IsValid) return View(user);

            try
            {
                user.IsActive = true;
                user.MustChangePassword = true;
                // FIX: Direct synchronous call
                _userService.AddUser(user);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating user: {ex.Message}");
                ModelState.AddModelError("", "Unable to create user. Please try again.");
                return View(user);
            }
        }

        public IActionResult Edit(string id)
        {
            try
            {
                var user = _userService.GetUserById(id);
                if (user == null) return NotFound();
                return View(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving user {id} for edit: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving the user.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string id, User user)
        {
            if (id != user.Id) return BadRequest();
            if (!ModelState.IsValid) return View(user);

            try
            {
                _userService.UpdateUser(id, user);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user {id}: {ex.Message}");
                ModelState.AddModelError("", "Unable to update user. Please try again.");
                return View(user);
            }
        }

        public IActionResult Delete(string id)
        {
            try
            {
                var user = _userService.GetUserById(id);
                if (user == null) return NotFound();
                return View(user);
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
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting user {id}: {ex.Message}");
                return StatusCode(500, "An error occurred while deleting the user.");
            }
        }
    }
}

using IncidentManagementsSystemNOSQL.Models;
using IncidentManagementsSystemNOSQL.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace IncidentManagementsSystemNOSQL.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await _userRepository.GetAll();
                return View(users);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching users: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving users.");
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            try
            {
                var user = await _userRepository.GetById(id);
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
        public async Task<IActionResult> Create(User user)
        {
            if (!ModelState.IsValid) return View(user);

            try
            {
                user.IsActive = true;
                user.MustChangePassword = true; 
                await _userRepository.AddUser(user);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating user: {ex.Message}");
                ModelState.AddModelError("", "Unable to create user. Please try again.");
                return View(user);
            }
        }

        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var user = await _userRepository.GetById(id);
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
        public async Task<IActionResult> Edit(string id, User user)
        {
            if (id != user.Id) return BadRequest();
            if (!ModelState.IsValid) return View(user);

            try
            {
                await _userRepository.UpdateUser(user);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user {id}: {ex.Message}");
                ModelState.AddModelError("", "Unable to update user. Please try again.");
                return View(user);
            }
        }

        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var user = await _userRepository.GetById(id);
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
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                await _userRepository.DeleteById(id);
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

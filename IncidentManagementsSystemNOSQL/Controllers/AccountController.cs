using System.Diagnostics;
using System.Security.Claims;
using IncidentManagementsSystemNOSQL.Models;
using IncidentManagementsSystemNOSQL.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IncidentManagementsSystemNOSQL.Controllers
{
    public class AccountController : Controller
    {   
        private readonly ILogger<AccountController> _logger;
        private readonly IUserService _userService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IDatabaseHealthService _databaseHealthService;
        private readonly IPasswordResetService _passwordResetService;

        public AccountController(
            ILogger<AccountController> logger,
            IUserService userService,
            IPasswordHasher passwordHasher,
            IDatabaseHealthService databaseHealthService,
            IPasswordResetService passwordResetService)
        {
            _logger = logger;
            _userService = userService;
            _passwordHasher = passwordHasher;
            _databaseHealthService = databaseHealthService;
            _passwordResetService = passwordResetService;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            try
            {
                var viewModel = new LoginViewModel { ReturnUrl = returnUrl };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to render login page with returnUrl {ReturnUrl}", returnUrl);
                return StatusCode(500, "An unexpected error occurred while loading the login page.");
            }
        }

        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(vm);
                }

                var user = _userService.GetUserByUsername(vm.Username);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Wrong username or password");
                    return View(vm);
                }

                var passwordValid = _passwordHasher.VerifyPassword(vm.Password, user.PasswordHash);
                if (!passwordValid || user.IsActive == false)
                {
                    ModelState.AddModelError(string.Empty, "Wrong username or password");
                    return View(vm);
                }

                var role = user.Role;

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.EmployeeId ?? string.Empty),
                    new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                    new Claim(ClaimTypes.Role, role.ToString()),
                    new Claim("employee_id", user.EmployeeId ?? string.Empty)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        AllowRefresh = true
                    });

                if (!string.IsNullOrWhiteSpace(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                {
                    return Redirect(vm.ReturnUrl);
                }

                if (role == Enums.UserRole.service_desk)
                {
                    return RedirectToAction("Index", "Dashboard");
                }
                else if (role == Enums.UserRole.employee)
                {
                    return RedirectToAction("Index", "Employee");
                }

                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to authenticate user {Username}", vm?.Username);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while processing your login.");
                return View(vm);
            }
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sign out current user");
                return StatusCode(500, "An unexpected error occurred while signing out.");
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult SetPassword()
        {
            return View(new SetPasswordViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetPassword(SetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = _userService.GetUserByUsername(model.Username);
            if (user == null)
            {
                ModelState.AddModelError("", "Username not found. Please contact the Service Desk.");
                return View(model);
            }

            var newHash = _passwordHasher.HashPassword(model.Password);
            _userService.SetPassword(user.Id, newHash);

            TempData["SuccessMessage"] = "Password set successfully. You can now log in.";
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to render access denied page");
                return StatusCode(500, "An unexpected error occurred while rendering the access denied page.");
            }
        }

        public IActionResult Index()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to render account index page");
                return StatusCode(500, "An unexpected error occurred while loading the account page.");
            }
        }

        public IActionResult Privacy()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to render privacy page");
                return StatusCode(500, "An unexpected error occurred while loading the privacy page.");
            }
        }

        [HttpGet("/health/mongo")]
        public IActionResult MongoHealth()
        {
            try
            {
                var healthResult = _databaseHealthService.CheckMongoHealth();
                if (healthResult.IsHealthy)
                {
                    return Ok(new { ok = true, collections = healthResult.Collections });
                }

                return StatusCode(500, new { ok = false, message = "MongoDB health check failed." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MongoDB health check failed");
                return StatusCode(500, new { ok = false, message = "MongoDB health check failed." });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            try
            {
                var model = new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier };
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to render error view");
                return StatusCode(500, "An unexpected error occurred while rendering the error page.");
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            _passwordResetService.IssueTokenByUsername(model.Username, ip);
            return View("ForgotPasswordConfirmation");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ResetPassword(string uid, string token)
        {
            if (string.IsNullOrWhiteSpace(uid) || string.IsNullOrWhiteSpace(token)) return BadRequest();
            return View(new ResetPasswordViewModel { UserId = uid, Token = token });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var ok = _passwordResetService.ResetPassword(model.UserId, model.Token, model.NewPassword);
            if (!ok)
            {
                ModelState.AddModelError("", "The reset link is invalid or has expired.");
                return View(model);
            }
            TempData["SuccessMessage"] = "Your password has been reset. Please sign in.";
            return RedirectToAction("Login");
        }
    }
}

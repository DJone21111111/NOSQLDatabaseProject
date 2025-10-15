using IncidentManagementsSystemNOSQL.Models;
using IncidentManagementsSystemNOSQL.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;

namespace IncidentManagementsSystemNOSQL.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IUserService _userService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IDatabaseHealthService _databaseHealthService;

        public AccountController(
            ILogger<AccountController> logger,
            IUserService userService,
            IPasswordHasher passwordHasher,
            IDatabaseHealthService databaseHealthService)
        {
            _logger = logger;
            _userService = userService;
            _passwordHasher = passwordHasher;
            _databaseHealthService = databaseHealthService;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            try
            {
                LoginViewModel viewModel = new LoginViewModel { ReturnUrl = returnUrl };
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
        public IActionResult Login(LoginViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(vm);
                }

                User? user = _userService.GetUserByUsername(vm.Username);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(vm);
                }

                bool passwordValid = _passwordHasher.VerifyPassword(vm.Password, user.PasswordHash);
                if (!passwordValid || user.IsActive == false)
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(vm);
                }

                List<Claim> claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Role, user.Role)
                };

                ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                ClaimsPrincipal principal = new ClaimsPrincipal(identity);

                HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        AllowRefresh = true
                    }).GetAwaiter().GetResult();

                if (!string.IsNullOrWhiteSpace(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                {
                    return Redirect(vm.ReturnUrl);
                }

                return RedirectToAction("Index", "Home");
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
        public IActionResult Logout()
        {
            try
            {
                HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).GetAwaiter().GetResult();
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
                MongoHealthResult healthResult = _databaseHealthService.CheckMongoHealth();
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
                ErrorViewModel model = new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier };
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to render error view");
                return StatusCode(500, "An unexpected error occurred while rendering the error page.");
            }
        }

    }
}

using IncidentManagementsSystemNOSQL.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Diagnostics;
using System.Security.Claims;

namespace IncidentManagementsSystemNOSQL.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IMongoDatabase _db;

        public AccountController(ILogger<AccountController> logger, IMongoDatabase db)
        {
            _logger = logger;
            _db = db;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
            => View(new LoginViewModel { ReturnUrl = returnUrl });

        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var users = _db.GetCollection<User>("users");
            var user = users.Find(u => u.UserName == vm.Username).FirstOrDefault();

            if (user is null || !VerifyPassword(vm.Password, user.PasswordHash) || user.IsActive == false)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(vm);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    AllowRefresh = true
                }).GetAwaiter().GetResult();

            if (!string.IsNullOrWhiteSpace(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                return Redirect(vm.ReturnUrl);

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).GetAwaiter().GetResult();
            return RedirectToAction(nameof(Login));
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied() => View();

        public IActionResult Index() => View();

        public IActionResult Privacy() => View();

        [HttpGet("/health/mongo")]
        public IActionResult MongoHealth()
        {
            _db.RunCommand<BsonDocument>("{ ping: 1 }");
            var collections = _db.ListCollectionNames().ToList();
            return Ok(new { ok = true, collections });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private static bool VerifyPassword(string password, string storedHash)
        {
            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<string>();
            var result = hasher.VerifyHashedPassword(null!, storedHash, password);
            return result == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Success
                   || result == Microsoft.AspNetCore.Identity.PasswordVerificationResult.SuccessRehashNeeded;
        }
    }
}

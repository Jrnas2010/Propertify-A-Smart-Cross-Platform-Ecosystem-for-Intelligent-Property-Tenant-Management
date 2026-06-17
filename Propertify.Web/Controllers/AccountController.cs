using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Propertify.Web.Data;
using Propertify.Web.Helpers;
using Propertify.Web.Models;
using System.Security.Claims;

namespace Propertify.Web.Controllers
{
    /// <summary>Handles user authentication: login, logout, and registration.</summary>
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context) => _context = context;

        /// <summary>Displays the login form.</summary>
        [HttpGet]
        public IActionResult Login() => View();

        /// <summary>
        /// Validates credentials, creates a cookie-based claims principal, and redirects to the dashboard.
        /// Returns the login view with a validation error on failure.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
                if (user != null && PasswordHelper.Verify(model.Password, user.Password))
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.Email),
                        new Claim(ClaimTypes.Role, user.Role),
                        new Claim("IsSystemAdmin", user.IsSystemAdmin ? "true" : "false"),
                        new Claim("FullName", user.FullName)
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    return RedirectToAction("Index", "Dashboard");
                }

                ModelState.AddModelError(string.Empty, " Invalid email or password.");
            }
            return View(model);
        }

        /// <summary>Signs the user out and redirects to the login page.</summary>
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        /// <summary>Displays the registration form.</summary>
        [HttpGet]
        public IActionResult Register() => View();

        /// <summary>
        /// Creates a new user account (hashed password), signs the user in automatically,
        /// and redirects to the dashboard. Returns the form with an error if the email is taken.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (_context.Users.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "This email is already registered.");
                    return View(model);
                }

                var user = new User
                {
                    FullName = $"{model.FirstName} {model.LastName}",
                    Email = model.Email,
                    Password = PasswordHelper.Hash(model.Password),
                    Role = "Owner"
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Auto-login after registration
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("IsSystemAdmin", user.IsSystemAdmin ? "true" : "false"),
                    new Claim("FullName", user.FullName)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return RedirectToAction("Index", "Dashboard");
            }
            return View(model);
        }
    }
}

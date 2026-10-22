using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using HabitTracker.Models;

namespace HabitTracker.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AuthController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe = false)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Please enter both email and password.";
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            if (user.IsBlocked)
            {
                ViewBag.Error = "Your account has been blocked by an administrator.";
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!,
                password,
                rememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            ViewBag.Error = "Invalid email or password.";
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string fullName, string email, string password, string confirmPassword)
        {
            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "All fields are required.";
                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                return View();
            }

            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                ViewBag.Error = "This email is already registered.";
                return View();
            }
            string firstName = fullName.Split(' ')[0];

            var newUser = new User
            {
                UserName = firstName + "_" + Guid.NewGuid().ToString().Substring(0, 3),
                Email = email,
                FullName = fullName,
                CreatedAt = DateTime.UtcNow,
                TotalPoints = 0,
                IsBlocked = false
            };

            var result = await _userManager.CreateAsync(newUser, password);

            if (result.Succeeded)
            {
                if (await _userManager.IsInRoleAsync(newUser, "User") == false)
                {
                    await _userManager.AddToRoleAsync(newUser, "User");
                }
                await _signInManager.SignInAsync(newUser, isPersistent: false);

                return RedirectToAction("Index", "Dashboard");
            }

            ViewBag.Error = string.Join(" ", result.Errors.Select(e => e.Description));
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
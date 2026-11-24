using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Habit_Tracker_Project.Models;
using Habit_Tracker_Project.Models.ViewModels;
using Habit_Tracker_Project;

namespace Habit_Tracker_Project.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IStringLocalizer<SharedResource> localizer)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _localizer = localizer;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (TempData["Error"] != null)
                ViewBag.Error = TempData["Error"];

            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Dashboard");

            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = _localizer[LocalizationKeys.Messages.InvalidLogin].Value;
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ViewBag.Error = _localizer[LocalizationKeys.Messages.AccountNotFound].Value;
                return View(model);
            }

            if (user.IsBlocked)
            {
                ViewBag.Error = _localizer[LocalizationKeys.Messages.AccountBlocked].Value;
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
                return RedirectToAction("Index", "Dashboard");

            ViewBag.Error = _localizer[LocalizationKeys.Messages.InvalidLogin].Value;
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (await _userManager.FindByEmailAsync(model.Email) != null)
            {
                ModelState.AddModelError(nameof(model.Email), _localizer[LocalizationKeys.Messages.EmailAlreadyRegistered]);
                return View(model);
            }

            string firstName = model.FullName.Split(' ')[0];
            var newUser = new User
            {
                UserName = firstName + "_" + Guid.NewGuid().ToString()[..3],
                Email = model.Email,
                FullName = model.FullName,
                CreatedAt = DateTime.UtcNow,
                TotalPoints = 0,
                IsBlocked = false
            };

            var result = await _userManager.CreateAsync(newUser, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            if (!await _userManager.IsInRoleAsync(newUser, "User"))
                await _userManager.AddToRoleAsync(newUser, "User");

            await _signInManager.SignInAsync(newUser, isPersistent: false);
            return RedirectToAction("Index", "Dashboard");
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
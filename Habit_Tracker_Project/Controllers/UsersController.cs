using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using HabitTracker.Models;
using System.Security.Claims;

namespace HabitTracker.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly UserManager<User> _userManager;

        public UsersController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.FindByIdAsync(GetCurrentUserId());
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(string fullName, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(GetCurrentUserId());
            if (user == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(fullName) && fullName != user.FullName)
            {
                user.FullName = fullName;
                await _userManager.UpdateAsync(user);
            }

            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

                if (!result.Succeeded)
                {
                    TempData["Error"] = string.Join(" ", result.Errors.Select(e => e.Description));
                    return RedirectToAction(nameof(Profile));
                }
            }

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Profile));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleBlock(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            if (user.Id == GetCurrentUserId())
            {
                TempData["Error"] = "You cannot block yourself.";
                return RedirectToAction(nameof(Manage));
            }

            user.IsBlocked = !user.IsBlocked;
            user.BlockedAt = user.IsBlocked ? DateTime.UtcNow : null;

            await _userManager.UpdateAsync(user);

            TempData["Success"] = user.IsBlocked ? "User blocked." : "User unblocked.";
            return RedirectToAction(nameof(Manage));
        }
    }
}
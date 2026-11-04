using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using HabitTracker.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using HabitTracker.Data;

namespace HabitTracker.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public UsersController(UserManager<User> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
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

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.Role = roles.Contains("Admin")
                ? "Admin"
                : roles.Contains("Moderator") ? "Moderator" : "User";

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(string fullName)
        {
            var user = await _userManager.FindByIdAsync(GetCurrentUserId());
            if (user == null) return NotFound();

            if (string.IsNullOrWhiteSpace(fullName))
            {
                TempData["Error"] = "Full name is required.";
                return RedirectToAction(nameof(Profile));
            }

            user.FullName = fullName.Trim();
            await _userManager.UpdateAsync(user);

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Profile));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Manage(string? searchString, string? status)
        {
            var users = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                users = users.Where(u =>
                    (u.Email != null && EF.Functions.ILike(u.Email, $"%{searchString}%")) ||
                    (u.FullName != null && EF.Functions.ILike(u.FullName, $"%{searchString}%")));
            }

            if (status == "active")
            {
                users = users.Where(u => !u.IsBlocked);
            }
            else if (status == "blocked")
            {
                users = users.Where(u => u.IsBlocked);
            }

            ViewBag.SearchString = searchString;
            ViewBag.Status = status;
            ViewBag.ModeratorIds = (await _userManager.GetUsersInRoleAsync("Moderator"))
                .Select(user => user.Id)
                .ToHashSet();

            return View(await users.OrderBy(u => u.Email).ToListAsync());
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("Users/Habits/{id}")]
        public async Task<IActionResult> Habits(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var habits = await _context.Habits
                .Include(h => h.Category)
                .Where(h => h.UserId == id && !h.IsDeleted)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();

            ViewBag.FullName = user.FullName;
            ViewBag.Email = user.Email;

            return View(habits);
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

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetModerator(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            if (await _userManager.IsInRoleAsync(user, "Moderator"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Moderator");
                TempData["Success"] = "Moderator role removed.";
            }
            else
            {
                await _userManager.AddToRoleAsync(user, "Moderator");
                TempData["Success"] = "User is now a moderator.";
            }

            return RedirectToAction(nameof(Manage));
        }
    }
}
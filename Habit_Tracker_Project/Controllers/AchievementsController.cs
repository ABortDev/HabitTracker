using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Habit_Tracker_Project.Data;
using Habit_Tracker_Project.Models;
using System.Security.Claims;

namespace Habit_Tracker_Project.Controllers
{
    [Authorize]
    public class AchievementsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AchievementsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();

            var allAchievements = await _context.Achievements.ToListAsync();

            var userAchievementIds = await _context.UserAchievements
                .Where(ua => ua.UserId == userId)
                .Select(ua => ua.AchievementId)
                .ToListAsync();

            ViewBag.UserAchievements = userAchievementIds;

            return View(allAchievements);
        }
    }
}
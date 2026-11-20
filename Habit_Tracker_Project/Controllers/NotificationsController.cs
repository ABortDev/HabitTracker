using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Habit_Tracker_Project.Data;
using Habit_Tracker_Project.Enums;
using System.Security.Claims;
using Microsoft.Extensions.Localization;

namespace Habit_Tracker_Project.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public NotificationsController(ApplicationDbContext context, IStringLocalizer<SharedResource> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        private string GetCurrentUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var items = await _context.Notifications
                .Where(n => n.UserId == userId && n.Type != NotificationType.Reminder)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> Feed()
        {
            var userId = GetCurrentUserId();

            var unreadCount = await _context.Notifications
                .CountAsync(n => n.UserId == userId
                    && !n.IsRead
                    && n.Type != NotificationType.Reminder);

            var items = await _context.Notifications
                .Where(n => n.UserId == userId && n.Type != NotificationType.Reminder)
                .OrderByDescending(n => n.CreatedAt)
                .Take(8)
                .Select(n => new
                {
                    n.Id,
                    n.Title,
                    n.Message,
                    n.IsRead,
                    createdAt = n.CreatedAt
                })
                .ToListAsync();

            var localizedItems = items.Select(notification => new
            {
                notification.Id,
                title = _localizer[notification.Title].Value,
                message = LocalizeMessage(notification.Message),
                notification.IsRead,
                notification.createdAt
            });

            return Json(new { unreadCount, items = localizedItems });
        }

        private string LocalizeMessage(string message)
        {
            if (message.StartsWith("You completed ", StringComparison.Ordinal))
            {
                var habitTitle = message["You completed ".Length..].TrimEnd('.');
                return _localizer["You completed {0}.", habitTitle].Value;
            }

            return _localizer[message].Value;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead(int id)
        {
            var userId = GetCurrentUserId();
            var item = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (item == null) return NotFound();

            item.IsRead = true;
            item.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
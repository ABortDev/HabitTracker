using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HabitTracker.Data;
using HabitTracker.Models;
using Habit_Tracker_Project.Enums;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace HabitTracker.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public ChatController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private string GetCurrentUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var messages = await _context.ChatMessages
                .Where(m => m.UserId == userId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();

            return View(messages);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return RedirectToAction(nameof(Index));

            var userId = GetCurrentUserId();

            _context.ChatMessages.Add(new ChatMessage
            {
                UserId = userId,
                SenderId = userId,
                Text = text.Trim(),
                CreatedAt = DateTime.UtcNow
            });

            var moderators = await _userManager.GetUsersInRoleAsync("Moderator");
            foreach (var moderator in moderators)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = moderator.Id,
                    Title = "New chat message",
                    Message = "A user sent a message.",
                    Type = NotificationType.System,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Moderator")]
        [HttpGet]
        public async Task<IActionResult> Inbox()
        {
            var threads = await _context.ChatMessages
                .Include(m => m.User)
                .GroupBy(m => m.UserId)
                .Select(g => g.OrderByDescending(m => m.CreatedAt).First())
                .ToListAsync();

            return View(threads);
        }

        [Authorize(Roles = "Moderator")]
        [HttpGet]
        public async Task<IActionResult> Thread(string userId)
        {
            var messages = await _context.ChatMessages
                .Include(m => m.User)
                .Where(m => m.UserId == userId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();

            ViewBag.ThreadUserId = userId;
            return View(messages);
        }

        [Authorize(Roles = "Moderator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(string userId, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return RedirectToAction(nameof(Thread), new { userId });

            var adminId = GetCurrentUserId();
            _context.ChatMessages.Add(new ChatMessage
            {
                UserId = userId,
                SenderId = adminId,
                Text = text.Trim(),
                CreatedAt = DateTime.UtcNow
            });

            _context.Notifications.Add(new Notification
            {
                UserId = userId,
                Title = "New message",
                Message = "A moderator replied to you.",
                Type = NotificationType.System,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Thread), new { userId });
        }
    }
}
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
            if (User.IsInRole("Moderator"))
                return RedirectToAction(nameof(Inbox));
            if (!User.IsInRole("User"))
                return Forbid();

            var userId = GetCurrentUserId();
            var messages = await _context.ChatMessages
                .Where(m => m.UserId == userId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();

            var unreadMessages = messages
                .Where(message => message.SenderId != userId && !message.IsRead)
                .ToList();
            foreach (var message in unreadMessages)
                message.IsRead = true;

            if (unreadMessages.Count > 0)
                await _context.SaveChangesAsync();

            ViewBag.CurrentUserId = userId;
            return View(messages);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(string text)
        {
            if (User.IsInRole("Moderator"))
                return RedirectToAction(nameof(Inbox));
            if (!User.IsInRole("User"))
                return Forbid();

            if (string.IsNullOrWhiteSpace(text) || text.Trim().Length > 4000)
            {
                TempData["Error"] = "Message must contain between 1 and 4000 characters.";
                return RedirectToAction(nameof(Index));
            }

            var userId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            _context.ChatMessages.Add(new ChatMessage
            {
                UserId = userId,
                SenderId = userId,
                Text = text.Trim(),
                CreatedAt = now
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
                    CreatedAt = now
                });
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = moderators.Count == 0
                ? "Message saved. No moderators are currently assigned."
                : "Message sent.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Moderator")]
        [HttpGet]
        public async Task<IActionResult> Inbox()
        {
            var latestMessageIds = _context.ChatMessages
                .GroupBy(message => message.UserId)
                .Select(group => group
                    .OrderByDescending(message => message.CreatedAt)
                    .ThenByDescending(message => message.Id)
                    .Select(message => message.Id)
                    .First());

            var threads = await _context.ChatMessages
                .Where(message => latestMessageIds.Contains(message.Id))
                .Include(m => m.User)
                .OrderByDescending(message => message.CreatedAt)
                .ThenByDescending(message => message.Id)
                .ToListAsync();

            var userIds = threads.Select(message => message.UserId).ToList();
            ViewBag.UnreadCounts = await _context.ChatMessages
                .Where(message => userIds.Contains(message.UserId)
                    && message.SenderId == message.UserId
                    && !message.IsRead)
                .GroupBy(message => message.UserId)
                .Select(group => new { UserId = group.Key, Count = group.Count() })
                .ToDictionaryAsync(item => item.UserId, item => item.Count);

            return View(threads);
        }

        [Authorize(Roles = "Moderator")]
        [HttpGet("Chat/Thread/{userId}")]
        public async Task<IActionResult> Thread(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var messages = await _context.ChatMessages
                .Include(m => m.User)
                .Where(m => m.UserId == userId)
                .OrderBy(m => m.CreatedAt)
                .ThenBy(m => m.Id)
                .ToListAsync();

            if (messages.Count == 0)
                return NotFound();

            var unreadMessages = messages
                .Where(message => message.SenderId == userId && !message.IsRead)
                .ToList();
            foreach (var message in unreadMessages)
                message.IsRead = true;

            if (unreadMessages.Count > 0)
                await _context.SaveChangesAsync();

            ViewBag.ThreadUserId = userId;
            ViewBag.ThreadUserName = user.FullName;
            ViewBag.ThreadUserEmail = user.Email;
            return View(messages);
        }

        [Authorize(Roles = "Moderator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(string userId, string text)
        {
            if (string.IsNullOrWhiteSpace(text) || text.Trim().Length > 4000)
            {
                TempData["Error"] = "Message must contain between 1 and 4000 characters.";
                return RedirectToAction(nameof(Thread), new { userId });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var hasConversation = await _context.ChatMessages.AnyAsync(message => message.UserId == userId);
            if (!hasConversation)
                return NotFound();

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
            TempData["Success"] = "Reply sent.";
            return RedirectToAction(nameof(Thread), new { userId });
        }
    }
}
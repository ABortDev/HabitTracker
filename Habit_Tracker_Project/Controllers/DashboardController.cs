using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using HabitTracker.Data;
using HabitTracker.Models;
using HabitTracker.Models.Enums;
using System.Security.Claims;

namespace HabitTracker.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
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

            var habits = await _context.Habits
                .Include(h => h.Category)
                .Include(h => h.Reminders)
                .Where(h => h.UserId == userId && !h.IsDeleted)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();

            ViewBag.Categories = await _context.HabitCategories.ToListAsync();

            return View(habits);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddHabit(string title, string description, int categoryId, HabitFrequency frequency, TimeOnly? reminderTime, DateOnly? endDate)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                TempData["Error"] = "Habit title is required.";
                return RedirectToAction(nameof(Index));
            }

            var userId = GetCurrentUserId();
            var defaultCategory = await _context.HabitCategories.FirstOrDefaultAsync();
            var finalCategoryId = categoryId > 0 ? categoryId : (defaultCategory?.Id ?? 1);

            var newHabit = new Habit
            {
                UserId = userId,
                CategoryId = finalCategoryId,
                Title = title,
                Description = description,
                Difficulty = DifficultyLevel.Medium,
                Frequency = frequency,
                Status = HabitStatus.Active,
                CreatedAt = DateTime.UtcNow,
                EndDate = endDate,
                IsDeleted = false
            };

            _context.Habits.Add(newHabit);
            await _context.SaveChangesAsync();

            if (reminderTime.HasValue)
            {
                var reminder = new HabitReminder
                {
                    HabitId = newHabit.Id,
                    ReminderTime = reminderTime.Value,
                    IsEnabled = true
                };
                _context.HabitReminders.Add(reminder);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Habit created successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditHabit(int id, string title, string description, int categoryId, HabitFrequency frequency, DateOnly? endDate, TimeOnly? reminderTime)
        {
            var userId = GetCurrentUserId();
            var habit = await _context.Habits
                .Include(h => h.Reminders)
                .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId && !h.IsDeleted);

            if (habit == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(title)) habit.Title = title;
            if (description != null) habit.Description = description;
            if (categoryId > 0) habit.CategoryId = categoryId;

            habit.Frequency = frequency;
            habit.EndDate = endDate;

            var existingReminder = habit.Reminders.FirstOrDefault();
            if (reminderTime.HasValue)
            {
                if (existingReminder != null)
                {
                    existingReminder.ReminderTime = reminderTime.Value;
                }
                else
                {
                    _context.HabitReminders.Add(new HabitReminder
                    {
                        HabitId = habit.Id,
                        ReminderTime = reminderTime.Value,
                        IsEnabled = true
                    });
                }
            }
            else if (existingReminder != null)
            {
                _context.HabitReminders.Remove(existingReminder);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Habit updated!";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteHabit(int id)
        {
            var userId = GetCurrentUserId();
            var habit = await _context.Habits
                .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId && !h.IsDeleted);

            if (habit == null) return NotFound();

            habit.IsDeleted = true;
            habit.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Habit deleted.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteHabit(int id)
        {
            var userId = GetCurrentUserId();
            var habit = await _context.Habits
                .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId && !h.IsDeleted);

            if (habit == null) return NotFound();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var existingLog = await _context.HabitLogs
                .FirstOrDefaultAsync(l => l.HabitId == id && l.Date == today);

            if (existingLog == null)
            {
                var log = new HabitLog
                {
                    HabitId = id,
                    Date = today,
                    ActualValue = 1,
                    CompletedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.HabitLogs.Add(log);

                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.TotalPoints += 10;
                }
            }

            if (habit.Frequency == HabitFrequency.Onetime)
            {
                habit.IsDeleted = true;
                habit.DeletedAt = DateTime.UtcNow;
                TempData["Success"] = "Task completed and removed from your list!";
            }
            else
            {
                TempData["Success"] = "Habit completed for today!";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}

using System.Text.Json;
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

        private static int GetPoints(DifficultyLevel difficulty) => difficulty switch
        {
            DifficultyLevel.Easy => 10,
            DifficultyLevel.Hard => 30,
            _ => 20
        };

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();

            var habits = await _context.Habits
                .Include(h => h.Category)
                .Include(h => h.Reminders)
                .Include(h => h.Logs)
                .Where(h => h.UserId == userId && !h.IsDeleted)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();

            ViewBag.Categories = await _context.HabitCategories.ToListAsync();

            return View(habits);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddHabit(
                string title,
                string? description,
                int categoryId,
                HabitFrequency frequency,
                DifficultyLevel difficulty,
                decimal? targetValue,
                string? unit,
                TimeOnly? reminderTime,
                DateOnly? endDate)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                TempData["Error"] = "Habit title is required.";
                return RedirectToAction(nameof(Index));
            }

            if (frequency == HabitFrequency.Onetime && !endDate.HasValue)
            {
                TempData["Error"] = "Target date is required for one-time tasks.";
                return RedirectToAction(nameof(Index));
            }

            if (targetValue.HasValue && (targetValue.Value <= 0 || targetValue.Value > 99999999))
            {
                TempData["Error"] = "Target value must be greater than 0.";
                return RedirectToAction(nameof(Index));
            }

            var userId = GetCurrentUserId();
            var defaultCategory = await _context.HabitCategories.FirstOrDefaultAsync();
            var finalCategoryId = categoryId > 0 ? categoryId : (defaultCategory?.Id ?? 1);

            var newHabit = new Habit
            {
                UserId = userId,
                CategoryId = finalCategoryId,
                Title = title.Trim(),
                Description = description,
                Difficulty = difficulty,
                Frequency = frequency,
                TargetValue = targetValue,
                Unit = targetValue.HasValue ? CleanUnit(unit) : null,
                Status = HabitStatus.Active,
                CreatedAt = DateTime.UtcNow,
                EndDate = endDate,
                IsDeleted = false
            };

            _context.Habits.Add(newHabit);
            await _context.SaveChangesAsync();

            if (reminderTime.HasValue)
            {
                _context.HabitReminders.Add(new HabitReminder
                {
                    HabitId = newHabit.Id,
                    ReminderTime = reminderTime.Value,
                    IsEnabled = true
                });
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Habit created successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditHabit(
            int id,
            string title,
            string? description,
            int categoryId,
            HabitFrequency frequency,
            DifficultyLevel difficulty,
            decimal? targetValue,
            string? unit,
            DateOnly? endDate,
            TimeOnly? reminderTime,
            bool reminderEnabled)
        {
            var userId = GetCurrentUserId();
            var habit = await _context.Habits
                .Include(h => h.Reminders)
                .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId && !h.IsDeleted);

            if (habit == null) return NotFound();

            if (frequency == HabitFrequency.Onetime && !endDate.HasValue)
            {
                TempData["Error"] = "Target date is required for one-time tasks.";
                return RedirectToAction(nameof(Index));
            }

            if (targetValue.HasValue && (targetValue.Value <= 0 || targetValue.Value > 99999999))
            {
                TempData["Error"] = "Target value must be greater than 0.";
                return RedirectToAction(nameof(Index));
            }

            if (!string.IsNullOrWhiteSpace(title)) habit.Title = title.Trim();
            if (description != null) habit.Description = description;
            if (categoryId > 0) habit.CategoryId = categoryId;

            habit.Frequency = frequency;
            habit.Difficulty = difficulty;
            habit.TargetValue = targetValue;
            habit.Unit = targetValue.HasValue ? CleanUnit(unit) : null;
            habit.EndDate = endDate;

            var existingReminder = habit.Reminders.FirstOrDefault();
            if (reminderTime.HasValue)
            {
                if (existingReminder != null)
                {
                    existingReminder.ReminderTime = reminderTime.Value;
                    existingReminder.IsEnabled = reminderEnabled;
                }
                else
                {
                    _context.HabitReminders.Add(new HabitReminder
                    {
                        HabitId = habit.Id,
                        ReminderTime = reminderTime.Value,
                        IsEnabled = reminderEnabled
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
        public async Task<IActionResult> CompleteHabit(int id, decimal? actualValue)
        {
            var userId = GetCurrentUserId();
            var habit = await _context.Habits
                .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId && !h.IsDeleted);

            if (habit == null) return NotFound();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var now = DateTime.UtcNow;

            var log = await _context.HabitLogs
                .FirstOrDefaultAsync(l => l.HabitId == id && l.Date == today);

            if (log != null && log.CompletedAt != null)
            {
                TempData["Error"] = "This habit is already completed today.";
                return RedirectToAction(nameof(Index));
            }

            var hasTarget = habit.TargetValue.HasValue && habit.TargetValue.Value > 0;

            decimal amount;
            if (hasTarget)
            {
                amount = actualValue ?? habit.TargetValue!.Value;
                if (amount <= 0)
                {
                    TempData["Error"] = "Enter a value greater than 0.";
                    return RedirectToAction(nameof(Index));
                }
            }
            else
            {
                amount = 1;
            }

            if (log == null)
            {
                log = new HabitLog
                {
                    HabitId = id,
                    Date = today,
                    ActualValue = 0,
                    Status = HabitLogStatus.NotCompleted
                };
                _context.HabitLogs.Add(log);
            }

            log.ActualValue = (log.ActualValue ?? 0) + amount;
            log.UpdatedAt = now;

            var isCompleted = !hasTarget || log.ActualValue >= habit.TargetValue!.Value;

            if (!isCompleted)
            {
                log.Status = HabitLogStatus.Partial;
                await _context.SaveChangesAsync();

                TempData["Success"] =
                    $"Progress saved: {log.ActualValue:0.##} / {habit.TargetValue:0.##} {habit.Unit}";
                return RedirectToAction(nameof(Index));
            }

            log.Status = HabitLogStatus.Completed;
            log.CompletedAt = now;

            var points = GetPoints(habit.Difficulty);

            _context.PointTransactions.Add(new PointTransaction
            {
                UserId = userId,
                Type = PointTransactionType.Earned,
                Points = points,
                HabitId = id,
                Description = $"Completed: {habit.Title}",
                CreatedAt = now
            });

            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.TotalPoints += points;
            }

            if (habit.Frequency == HabitFrequency.Onetime)
            {
                habit.IsDeleted = true;
                habit.DeletedAt = now;
                TempData["Success"] = $"Task completed (+{points} pts) and removed from your list!";
            }
            else
            {
                TempData["Success"] = $"Habit completed for today! +{points} pts";
            }

            await _context.SaveChangesAsync();
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

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = GetCurrentUserId();

            var habit = await _context.Habits
                .Include(h => h.Category)
                .Include(h => h.Reminders)
                .Include(h => h.Logs)
                .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId && !h.IsDeleted);

            if (habit == null) return NotFound();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var dates = habit.Logs
                .Where(l => l.CompletedAt != null)
                .Select(l => l.Date)
                .ToHashSet();

            var day = dates.Contains(today) ? today : today.AddDays(-1);
            var streak = 0;
            while (dates.Contains(day))
            {
                streak++;
                day = day.AddDays(-1);
            }

            ViewBag.CompletedToday = dates.Contains(today);
            ViewBag.TotalCompletions = dates.Count;
            ViewBag.Streak = streak;
            ViewBag.LastCompleted = dates.Count > 0 ? dates.Max() : (DateOnly?)null;

            ViewBag.PlannedJson = JsonSerializer.Serialize(
            habit.Logs
                .Where(l => l.CompletedAt == null && l.Status == HabitLogStatus.NotCompleted)
                .Select(l => l.Date.ToString("yyyy-MM-dd"))
                .ToList());

            ViewBag.PartialJson = JsonSerializer.Serialize(
                habit.Logs
                    .Where(l => l.CompletedAt == null && l.Status == HabitLogStatus.Partial)
                    .Select(l => l.Date.ToString("yyyy-MM-dd"))
                    .ToList());

            ViewBag.CompletedJson = JsonSerializer.Serialize(
                dates.Select(d => d.ToString("yyyy-MM-dd")).ToList());

            return View(habit);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleScheduleDate(int id, DateOnly date)
        {
            var userId = GetCurrentUserId();

            var habit = await _context.Habits
                .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId && !h.IsDeleted);

            if (habit == null) return NotFound();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            if (date < today)
                return BadRequest(new { error = "You can only plan today or future dates." });

            if (habit.EndDate.HasValue && date > habit.EndDate.Value)
                return BadRequest(new { error = "This date is after the habit's end date." });

            var log = await _context.HabitLogs
                .FirstOrDefaultAsync(l => l.HabitId == id && l.Date == date);

            bool planned;

            if (log == null)
            {
                _context.HabitLogs.Add(new HabitLog
                {
                    HabitId = id,
                    Date = date,
                    Status = HabitLogStatus.NotCompleted,
                    UpdatedAt = DateTime.UtcNow
                });
                planned = true;
            }
            else if (log.CompletedAt == null && log.Status == HabitLogStatus.NotCompleted)
            {
                _context.HabitLogs.Remove(log);
                planned = false;
            }
            else
            {
                return BadRequest(new { error = "This day already has progress and cannot be unplanned." });
            }

            await _context.SaveChangesAsync();
            return Json(new { planned });
        }
        private static string? CleanUnit(string? unit) =>
            string.IsNullOrWhiteSpace(unit) ? null : unit.Trim();
    }
}
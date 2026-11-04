using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using HabitTracker.Data;
using HabitTracker.Models;
using HabitTracker.Models.Enums;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Habit_Tracker_Project.Enums;
using System.Globalization;

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
        public async Task<IActionResult> Index(string? searchString, int? categoryId)
        {
            var userId = GetCurrentUserId();

            var habits = _context.Habits
                .Include(h => h.Category)
                .Include(h => h.Reminders)
                .Include(h => h.Logs)
                .Where(h => h.UserId == userId && !h.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                habits = habits.Where(h => EF.Functions.ILike(h.Title, $"%{searchString}%"));
            }
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                habits = habits.Where(h => h.CategoryId == categoryId.Value);
            }
            ViewBag.Categories = await _context.HabitCategories
                .OrderBy(c => c.Name)
                .ToListAsync();
            ViewBag.SearchString = searchString;
            ViewBag.CategoryId = categoryId;
            return View(await habits.OrderByDescending(h => h.CreatedAt).ToListAsync());
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
        public async Task<IActionResult> CompleteHabit(int id, decimal? actualValue, DateOnly? date)
        {
            var userId = GetCurrentUserId();
            var habit = await _context.Habits
                .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId && !h.IsDeleted);

            if (habit == null) return NotFound();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var now = DateTime.UtcNow;
            var targetDate = date ?? today;

            IActionResult Back() => date.HasValue
                ? RedirectToAction(nameof(Details), new { id })
                : RedirectToAction(nameof(Index));

            if (targetDate > today)
            {
                TempData["Error"] = "You can complete only today or a past date.";
                return Back();
            }

            if (habit.EndDate.HasValue && targetDate > habit.EndDate.Value)
            {
                TempData["Error"] = "This date is after the habit's end date.";
                return Back();
            }

            var completedDates = await _context.HabitLogs
                .Where(l => l.HabitId == id && l.CompletedAt != null && l.Date != targetDate)
                .Select(l => l.Date)
                .ToListAsync();

            bool alreadyInPeriod = habit.Frequency switch
            {
                HabitFrequency.Weekly => completedDates.Any(d =>
                    ISOWeek.GetYear(d.ToDateTime(TimeOnly.MinValue)) == ISOWeek.GetYear(targetDate.ToDateTime(TimeOnly.MinValue)) &&
                    ISOWeek.GetWeekOfYear(d.ToDateTime(TimeOnly.MinValue)) == ISOWeek.GetWeekOfYear(targetDate.ToDateTime(TimeOnly.MinValue))),
                HabitFrequency.Monthly => completedDates.Any(d => d.Year == targetDate.Year && d.Month == targetDate.Month),
                HabitFrequency.Yearly => completedDates.Any(d => d.Year == targetDate.Year),
                HabitFrequency.Onetime => completedDates.Count > 0,
                _ => false
            };

            if (alreadyInPeriod)
            {
                TempData["Error"] = "This habit is already completed for this period.";
                return Back();
            }

            var log = await _context.HabitLogs
                .FirstOrDefaultAsync(l => l.HabitId == id && l.Date == targetDate);

            if (log != null && log.CompletedAt != null)
            {
                TempData["Error"] = "This day is already completed.";
                return Back();
            }

            var hasTarget = habit.TargetValue.HasValue && habit.TargetValue.Value > 0;

            decimal amount;
            if (hasTarget)
            {
                if (!actualValue.HasValue || actualValue.Value <= 0)
                {
                    TempData["Error"] = "Enter how much you completed.";
                    return Back();
                }

                amount = actualValue.Value;
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
                    Date = targetDate,
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
                return Back();
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

            _context.Notifications.Add(new Notification
            {
                UserId = userId,
                HabitId = id,
                Title = "Habit completed",
                Message = $"You completed {habit.Title}.",
                Type = NotificationType.System,
                IsRead = false,
                CreatedAt = now
            });

            if (habit.Frequency == HabitFrequency.Onetime)
            {
                habit.IsDeleted = true;
                habit.DeletedAt = now;
                TempData["Success"] = $"Task completed (+{points} pts) and removed from your list!";
            }
            else if (targetDate == today)
            {
                TempData["Success"] = $"Habit completed for today! +{points} pts";
            }
            else
            {
                TempData["Success"] = $"Habit completed for {targetDate:yyyy-MM-dd}! +{points} pts";
            }

            await _context.SaveChangesAsync();
            await AwardAchievements(userId, user);
            await _context.SaveChangesAsync();
            return Back();
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

        private async Task AwardAchievements(string userId, User? user)
        {
            var catalog = await _context.Achievements.ToListAsync();
            if (catalog.Count == 0) return;

            var owned = await _context.UserAchievements
                .Where(ua => ua.UserId == userId)
                .Select(ua => ua.AchievementId)
                .ToListAsync();

            var completedCount = await _context.HabitLogs
                .CountAsync(l => l.Habit.UserId == userId && l.CompletedAt != null);

            var points = user?.TotalPoints ?? 0;

            void Give(string title)
            {
                var achievement = catalog.FirstOrDefault(a => a.Title == title);
                if (achievement == null || owned.Contains(achievement.Id)) return;

                _context.UserAchievements.Add(new UserAchievement
                {
                    UserId = userId,
                    AchievementId = achievement.Id,
                    AwardedAt = DateTime.UtcNow
                });

                _context.Notifications.Add(new Notification
                {
                    UserId = userId,
                    Title = "New achievement",
                    Message = achievement.Title,
                    Type = NotificationType.Achievement,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (completedCount >= 1) Give("First Step");
            if (completedCount >= 10) Give("10 Completions");
            if (points >= 100) Give("100 Points");
        }
    }
}
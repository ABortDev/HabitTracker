using Habit_Tracker_Project.Enums;
using Habit_Tracker_Project.Data;
using Habit_Tracker_Project.Models;
using Habit_Tracker_Project.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Habit_Tracker_Project.Services
{
    public class ReminderDispatcher : BackgroundService
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration _config;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ReminderDispatcher> _logger;
        private static readonly TimeZoneInfo Kyiv =
            TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time");

        public ReminderDispatcher(
            IHttpClientFactory httpFactory,
            IConfiguration config,
            IServiceScopeFactory scopeFactory,
            ILogger<ReminderDispatcher> logger)
        {
            _httpFactory = httpFactory;
            _config = config;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessDueRemindersAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Reminder dispatcher failed");
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        private async Task ProcessDueRemindersAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var nowKyiv = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Kyiv);
            var today = DateOnly.FromDateTime(nowKyiv);
            var nowTime = TimeOnly.FromDateTime(nowKyiv);

            var startOfTodayKyiv = today.ToDateTime(TimeOnly.MinValue);
            var startUtc = TimeZoneInfo.ConvertTimeToUtc(
                DateTime.SpecifyKind(startOfTodayKyiv, DateTimeKind.Unspecified), Kyiv);

            var reminders = await db.HabitReminders
                .Include(r => r.Habit)
                    .ThenInclude(h => h.Category)
                .Where(r => r.IsEnabled
                            && !r.Habit.IsDeleted
                            && r.Habit.Status == HabitStatus.Active)
                .ToListAsync(ct);

            var sentHabitIds = new HashSet<int>();

            foreach (var reminder in reminders)
            {
                var habit = reminder.Habit;
                var user = await db.Users.FirstOrDefaultAsync(u => u.Id == habit.UserId, ct);
                var firstName = string.IsNullOrWhiteSpace(user?.FullName)
                    ? "there"
                    : user.FullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];

                if (Math.Abs((reminder.ReminderTime - nowTime).TotalMinutes) > 1)
                    continue;

                if (!IsDueToday(habit, today))
                    continue;

                if (sentHabitIds.Contains(habit.Id))
                    continue;

                var alreadySent = await db.Notifications.AnyAsync(n =>
                    n.HabitId == habit.Id
                    && n.UserId == habit.UserId
                    && n.Type == NotificationType.Reminder
                    && n.CreatedAt >= startUtc, ct);

                if (alreadySent)
                    continue;

                sentHabitIds.Add(habit.Id);

                var categoryName = habit.Category != null ? habit.Category.Name : "No category";
                var details =
                    $"{firstName}, don't forget to: {habit.Title}\n" +
                    $"Category: {categoryName}\n" +
                    $"Time: {reminder.ReminderTime:HH:mm}\n" +
                    $"Frequency: {habit.Frequency}\n" +
                    $"Difficulty: {habit.Difficulty}\n" +
                    $"Status: {habit.Status}" +
                    (habit.EndDate.HasValue ? $"\nDeadline: {habit.EndDate:yyyy-MM-dd}" : "") +
                    (string.IsNullOrWhiteSpace(habit.Description) ? "" : $"\n{habit.Description}");

                db.Notifications.Add(new Notification
                {
                    UserId = habit.UserId,
                    HabitId = habit.Id,
                    Title = habit.Title,
                    Message = details,
                    Type = NotificationType.Reminder,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
                await db.SaveChangesAsync(ct);

                if (user?.TelegramChatId != null)
                {
                    var token = _config["Telegram:BotToken"];
                    if (!string.IsNullOrWhiteSpace(token))
                    {
                        var client = _httpFactory.CreateClient();
                        var api = $"https://api.telegram.org/bot{token}";
                        try
                        {
                            await TelegramBotService.SendAsync(
                                client,
                                api,
                                user.TelegramChatId.Value,
                                details,
                                ct);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Telegram send failed for user {UserId}", user.Id);
                        }
                    }
                }
            }
        }

        private static bool IsDueToday(Habit habit, DateOnly today)
        {
            return habit.Frequency switch
            {
                HabitFrequency.Daily => true,
                HabitFrequency.Onetime => habit.EndDate == today,
                HabitFrequency.Weekly =>
                    DateOnly.FromDateTime(habit.CreatedAt).DayOfWeek == today.DayOfWeek,
                HabitFrequency.Monthly =>
                    DateOnly.FromDateTime(habit.CreatedAt).Day == today.Day,
                HabitFrequency.Yearly =>
                    DateOnly.FromDateTime(habit.CreatedAt).Month == today.Month
                    && DateOnly.FromDateTime(habit.CreatedAt).Day == today.Day,
                _ => true
            };
        }
    }
}
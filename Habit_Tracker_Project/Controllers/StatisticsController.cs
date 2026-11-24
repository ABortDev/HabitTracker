using Habit_Tracker_Project.Data;
using Habit_Tracker_Project.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using System.Globalization;
using Microsoft.Extensions.Localization;

namespace Habit_Tracker_Project.Controllers
{
    [Authorize]
    public class StatisticsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public StatisticsController(ApplicationDbContext context, IStringLocalizer<SharedResource> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var completionDates = await _context.HabitLogs
                .Where(l => l.Habit.UserId == userId && l.CompletedAt != null)
                .Select(l => l.Date)
                .ToListAsync();

            var rawTransactions = await _context.PointTransactions
                .Where(p => p.UserId == userId && p.Type != PointTransactionType.Spent)
                .Select(p => new { p.CreatedAt, p.Points })
                .ToListAsync();

            var transactions = rawTransactions
                .Select(p => (Date: DateOnly.FromDateTime(p.CreatedAt), p.Points))
                .ToList();

            int PointsBetween(DateOnly from, DateOnly to) =>
                transactions.Where(t => t.Date >= from && t.Date <= to).Sum(t => t.Points);

            int CompletionsBetween(DateOnly from, DateOnly to) =>
                completionDates.Count(d => d >= from && d <= to);

            var dateSet = completionDates.ToHashSet();
            var day = dateSet.Contains(today) ? today : today.AddDays(-1);
            var streak = 0;
            while (dateSet.Contains(day))
            {
                streak++;
                day = day.AddDays(-1);
            }

            var weekStart = today.AddDays(-(((int)today.DayOfWeek + 6) % 7));
            var weekChart = Enumerable.Range(0, 7)
                .Select(i => PointsBetween(weekStart.AddDays(i), weekStart.AddDays(i)))
                .ToArray();

            var week = new
            {
                points = weekChart.Sum(),
                completions = CompletionsBetween(weekStart, weekStart.AddDays(6)),
                streak,
                label = _localizer["This week"].Value,
                chart = weekChart,
                labels = Enumerable.Range(0, 7)
                    .Select(i => weekStart.AddDays(i).ToString("ddd", CultureInfo.CurrentCulture))
                    .ToArray()
            };

            var monthStart = new DateOnly(today.Year, today.Month, 1);
            var daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
            var monthEnd = monthStart.AddDays(daysInMonth - 1);
            var bucketCount = (int)Math.Ceiling(daysInMonth / 7.0);

            var monthRanges = Enumerable.Range(0, bucketCount)
                .Select(b =>
                {
                    var from = monthStart.AddDays(b * 7);
                    var to = from.AddDays(6) > monthEnd ? monthEnd : from.AddDays(6);
                    return (From: from, To: to);
                })
                .ToList();

            var monthChart = monthRanges.Select(r => PointsBetween(r.From, r.To)).ToArray();

            var month = new
            {
                points = PointsBetween(monthStart, monthEnd),
                completions = CompletionsBetween(monthStart, monthEnd),
                streak,
                label = _localizer["This month"].Value,
                chart = monthChart,
                labels = monthRanges.Select(r => $"{r.From.Day}-{r.To.Day}").ToArray()
            };

            var yearStart = new DateOnly(today.Year, 1, 1);
            var yearEnd = new DateOnly(today.Year, 12, 31);
            var yearChart = Enumerable.Range(1, 12)
                .Select(m =>
                {
                    var from = new DateOnly(today.Year, m, 1);
                    var to = new DateOnly(today.Year, m, DateTime.DaysInMonth(today.Year, m));
                    return PointsBetween(from, to);
                })
                .ToArray();

            var year = new
            {
                points = PointsBetween(yearStart, yearEnd),
                completions = CompletionsBetween(yearStart, yearEnd),
                streak,
                label = _localizer["This year"].Value,
                chart = yearChart,
                labels = Enumerable.Range(1, 12)
                    .Select(m => new DateOnly(today.Year, m, 1).ToString("MMM", CultureInfo.CurrentCulture))
                    .ToArray()
            };

            ViewBag.StatisticsJson = JsonSerializer.Serialize(new { week, month, year });

            return View();
        }
    }
}
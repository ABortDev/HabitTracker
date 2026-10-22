using Habit_Tracker_Project.Models;
using HabitTracker.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace HabitTracker.Models
{
    public class Habit
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;
        public int CategoryId { get; set; }
        public HabitCategory Category { get; set; } = null!;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DifficultyLevel Difficulty { get; set; }
        public HabitFrequency Frequency { get; set; } = HabitFrequency.Daily;
        public decimal? TargetValue { get; set; }
        public string? Unit { get; set; }
        public DateOnly? EndDate { get; set; }
        public HabitStatus Status { get; set; } = HabitStatus.Active;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public ICollection<HabitLog> Logs { get; set; } = new List<HabitLog>();
        public ICollection<HabitReminder> Reminders { get; set; } = new List<HabitReminder>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}

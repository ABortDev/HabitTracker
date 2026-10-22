using Habit_Tracker_Project.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace HabitTracker.Models
{
    public class User : IdentityUser
    {
        public string? FullName { get; set; }
        public bool IsBlocked { get; set; } = false;
        public DateTime? BlockedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int TotalPoints { get; set; } = 0;
        public ICollection<Habit> Habits { get; set; } = new List<Habit>();
        public ICollection<PointTransaction> PointTransactions { get; set; } = new List<PointTransaction>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}

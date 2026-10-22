using HabitTracker.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Habit_Tracker_Project.Models
{
    public class UserAchievement
    {
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;
        public int AchievementId { get; set; }
        public Achievement Achievement { get; set; } = null!;
        public DateTime AwardedAt { get; set; } = DateTime.UtcNow;
    }
}

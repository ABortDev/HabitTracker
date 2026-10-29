using System.ComponentModel.DataAnnotations;

namespace HabitTracker.Models
{
    public class Achievement
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
    }

}

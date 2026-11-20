using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Habit_Tracker_Project.Models
{
    public class HabitCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ICollection<Habit> Habits { get; set; } = new List<Habit>();
    }
}

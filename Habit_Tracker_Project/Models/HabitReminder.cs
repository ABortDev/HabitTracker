using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace HabitTracker.Models
{
    public class HabitReminder
    {
        public int Id { get; set; }
        public int HabitId { get; set; }
        public Habit Habit { get; set; } = null!;
        public TimeOnly ReminderTime { get; set; }
        public bool IsEnabled { get; set; } = true;
    }
}

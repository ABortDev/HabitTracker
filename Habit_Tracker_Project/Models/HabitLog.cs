using HabitTracker.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace HabitTracker.Models
{
    public class HabitLog
    {
        public int Id { get; set; }
        public int HabitId { get; set; }
        public Habit Habit { get; set; } = null!;
        public DateOnly Date { get; set; }
        public HabitLogStatus Status { get; set; } = HabitLogStatus.NotCompleted;
        public decimal? ActualValue { get; set; }
        public string? Comment { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

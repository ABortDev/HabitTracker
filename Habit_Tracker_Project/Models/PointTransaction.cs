using HabitTracker.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace HabitTracker.Models
{
    public class PointTransaction
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;
        public PointTransactionType Type { get; set; }
        public int Points { get; set; }
        public int? HabitId { get; set; }
        public Habit? Habit { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

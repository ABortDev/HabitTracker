
using HabitTracker.Models;
using Habit_Tracker_Project.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace HabitTracker.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Habit> Habits { get; set; }
        public DbSet<HabitCategory> HabitCategories { get; set; }
        public DbSet<HabitLog> HabitLogs { get; set; }
        public DbSet<HabitReminder> HabitReminders { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<PointTransaction> PointTransactions { get; set; }
        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<UserAchievement> UserAchievements { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Habit>()
                .HasOne(h => h.User)
                .WithMany(u => u.Habits)
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Habit>()
                .HasOne(h => h.Category)
                .WithMany(c => c.Habits)
                .HasForeignKey(h => h.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<HabitLog>()
                .HasOne(l => l.Habit)
                .WithMany(h => h.Logs)
                .HasForeignKey(l => l.HabitId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<HabitReminder>()
                .HasOne(r => r.Habit)
                .WithMany(h => h.Reminders)
                .HasForeignKey(r => r.HabitId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Notification>()
                .HasOne(n => n.Habit)
                .WithMany(h => h.Notifications)
                .HasForeignKey(n => n.HabitId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<PointTransaction>()
                .HasOne(p => p.User)
                .WithMany(u => u.PointTransactions)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PointTransaction>()
                .HasOne(p => p.Habit)
                .WithMany()
                .HasForeignKey(p => p.HabitId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<UserAchievement>()
                .HasKey(ua => new { ua.UserId, ua.AchievementId });

            builder.Entity<UserAchievement>()
                .HasOne(ua => ua.User)
                .WithMany()
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserAchievement>()
                .HasOne(ua => ua.Achievement)
                .WithMany(a => a.UserAchievements)
                .HasForeignKey(ua => ua.AchievementId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<HabitLog>()
                .HasIndex(l => new { l.HabitId, l.Date })
                .IsUnique();

            builder.Entity<Habit>().ToTable("Habits");
            builder.Entity<HabitCategory>().ToTable("HabitCategories");
            builder.Entity<HabitLog>().ToTable("HabitLogs");
            builder.Entity<HabitReminder>().ToTable("HabitReminders");
            builder.Entity<Notification>().ToTable("Notifications");
            builder.Entity<PointTransaction>().ToTable("PointTransactions");
            builder.Entity<Achievement>().ToTable("Achievements");
            builder.Entity<UserAchievement>().ToTable("UserAchievements");

            builder.Entity<Habit>()
                .Property(h => h.TargetValue)
                .HasPrecision(10, 2);

            builder.Entity<HabitLog>()
                .Property(l => l.ActualValue)
                .HasPrecision(10, 2);

            builder.Entity<Habit>()
                .Property(h => h.Title)
                .IsRequired()
                .HasMaxLength(150);

            builder.Entity<HabitCategory>()
                .Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Entity<Achievement>()
                .Property(a => a.Title)
                .IsRequired()
                .HasMaxLength(150);

            builder.Entity<Notification>()
                .Property(n => n.Title)
                .IsRequired()
                .HasMaxLength(150);
        }
    }
}

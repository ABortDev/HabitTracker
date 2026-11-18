using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using HabitTracker.Models; 
using HabitTracker.Models.Enums; 

namespace HabitTracker.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                if (!context.Achievements.Any())
                {
                    context.Achievements.AddRange(
                        new Achievement { Title = "First Step", Description = "Complete your first habit." },
                        new Achievement { Title = "10 Completions", Description = "Complete habits 10 times." },
                        new Achievement { Title = "100 Points", Description = "Earn 100 points." }
                    );
                    context.SaveChanges();
                }
                if (!context.HabitCategories.Any())
                {
                    context.HabitCategories.AddRange(
                        new HabitCategory { Name = "Health & Fitness", Description = "Habits related to physical health, sports, nutrition, and sleep" },
                        new HabitCategory { Name = "Productivity", Description = "Work, study, time management, and focus routines" },
                        new HabitCategory { Name = "Mental Health", Description = "Meditation, breathing exercises, journaling, and relaxation" },
                        new HabitCategory { Name = "Learning", Description = "Reading books, learning languages, acquiring new skills, and self-growth" },
                        new HabitCategory { Name = "Finance", Description = "Budget tracking, saving money, and investments" },
                        new HabitCategory { Name = "Lifestyle", Description = "Home care, self-care routines, organization, and daily habits" }
                    );
                    context.SaveChanges();
                }
                if (!context.Habits.Any())
                {
                    string testUserId = "d1cec91b-153f-41c1-ab89-483fcde39bd7";

                    context.Habits.AddRange(
                        new Habit
                        {
                            UserId = testUserId,
                            CategoryId = 1,
                            Title = "Drink 2L of water",
                            Description = "Stay hydrated throughout the day",
                            Frequency = HabitFrequency.Daily,
                            TargetValue = 2,
                            Unit = "liters",
                            Status = HabitStatus.Active,
                            CreatedAt = DateTime.UtcNow,
                            IsDeleted = false,
                            Difficulty = DifficultyLevel.Easy 
                        },
                        new Habit
                        {
                            UserId = testUserId,
                            CategoryId = 1,
                            Title = "Morning Run",
                            Description = "Run outside to wake up",
                            Frequency = HabitFrequency.Weekly,
                            TargetValue = 5,
                            Unit = "km",
                            Status = HabitStatus.Active,
                            CreatedAt = DateTime.UtcNow,
                            IsDeleted = false,
                            Difficulty = DifficultyLevel.Hard
                        },

                        new Habit
                        {
                            UserId = testUserId,
                            CategoryId = 2,
                            Title = "Pomodoro Sessions",
                            Description = "Focus on deep work",
                            Frequency = HabitFrequency.Daily,
                            TargetValue = 4,
                            Unit = "sessions",
                            Status = HabitStatus.Active,
                            CreatedAt = DateTime.UtcNow,
                            IsDeleted = false,
                            Difficulty = DifficultyLevel.Medium
                        },

                        new Habit
                        {
                            UserId = testUserId,
                            CategoryId = 3,
                            Title = "Meditation",
                            Description = "Mindfulness practice",
                            Frequency = HabitFrequency.Daily,
                            TargetValue = 10,
                            Unit = "minutes",
                            Status = HabitStatus.Active,
                            CreatedAt = DateTime.UtcNow,
                            IsDeleted = false,
                            Difficulty = DifficultyLevel.Medium
                        },

                        new Habit
                        {
                            UserId = testUserId,
                            CategoryId = 4,
                            Title = "Read a Book",
                            Description = "Read non-fiction or fiction",
                            Frequency = HabitFrequency.Daily,
                            TargetValue = 20,
                            Unit = "pages",
                            Status = HabitStatus.Active,
                            CreatedAt = DateTime.UtcNow,
                            IsDeleted = false,
                            Difficulty = DifficultyLevel.Easy
                        },

                        new Habit
                        {
                            UserId = testUserId,
                            CategoryId = 5,
                            Title = "Track Expenses",
                            Description = "Log every purchase",
                            Frequency = HabitFrequency.Daily,
                            TargetValue = 1,
                            Unit = "time",
                            Status = HabitStatus.Active,
                            CreatedAt = DateTime.UtcNow,
                            IsDeleted = false,
                            Difficulty = DifficultyLevel.Medium
                        }
                    );

                    context.SaveChanges();     
                }
            }
        }
    }
}
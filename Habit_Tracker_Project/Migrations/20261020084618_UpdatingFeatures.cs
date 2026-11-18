using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Habit_Tracker_Project.Migrations
{
    /// <inheritdoc />
    public partial class UpdatingFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DayOfWeek",
                table: "HabitSchedules");

            migrationBuilder.DropColumn(
                name: "DayOfWeek",
                table: "HabitReminders");

            migrationBuilder.DropColumn(
                name: "ReminderTime",
                table: "HabitReminders");

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduleDate",
                table: "HabitSchedules",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ReminderDate",
                table: "HabitReminders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScheduleDate",
                table: "HabitSchedules");

            migrationBuilder.DropColumn(
                name: "ReminderDate",
                table: "HabitReminders");

            migrationBuilder.AddColumn<int>(
                name: "DayOfWeek",
                table: "HabitSchedules",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DayOfWeek",
                table: "HabitReminders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "ReminderTime",
                table: "HabitReminders",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));
        }
    }
}

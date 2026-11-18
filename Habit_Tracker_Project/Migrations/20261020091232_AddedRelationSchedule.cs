using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Habit_Tracker_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddedRelationSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HabitScheduleId",
                table: "HabitReminders",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HabitReminders_HabitScheduleId",
                table: "HabitReminders",
                column: "HabitScheduleId");

            migrationBuilder.AddForeignKey(
                name: "FK_HabitReminders_HabitSchedules_HabitScheduleId",
                table: "HabitReminders",
                column: "HabitScheduleId",
                principalTable: "HabitSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HabitReminders_HabitSchedules_HabitScheduleId",
                table: "HabitReminders");

            migrationBuilder.DropIndex(
                name: "IX_HabitReminders_HabitScheduleId",
                table: "HabitReminders");

            migrationBuilder.DropColumn(
                name: "HabitScheduleId",
                table: "HabitReminders");
        }
    }
}

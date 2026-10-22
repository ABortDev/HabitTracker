using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Habit_Tracker_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationHabitRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RelatedHabitId",
                table: "Notifications",
                column: "RelatedHabitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Habits_RelatedHabitId",
                table: "Notifications",
                column: "RelatedHabitId",
                principalTable: "Habits",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Habits_RelatedHabitId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_RelatedHabitId",
                table: "Notifications");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Habit_Tracker_Project.Migrations
{
    /// <inheritdoc />
    public partial class NewRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PointTransactions_HabitLogs_HabitLogId",
                table: "PointTransactions");

            migrationBuilder.DropIndex(
                name: "IX_PointTransactions_HabitLogId",
                table: "PointTransactions");

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_HabitLogId",
                table: "PointTransactions",
                column: "HabitLogId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PointTransactions_HabitLogs_HabitLogId",
                table: "PointTransactions",
                column: "HabitLogId",
                principalTable: "HabitLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PointTransactions_HabitLogs_HabitLogId",
                table: "PointTransactions");

            migrationBuilder.DropIndex(
                name: "IX_PointTransactions_HabitLogId",
                table: "PointTransactions");

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_HabitLogId",
                table: "PointTransactions",
                column: "HabitLogId");

            migrationBuilder.AddForeignKey(
                name: "FK_PointTransactions_HabitLogs_HabitLogId",
                table: "PointTransactions",
                column: "HabitLogId",
                principalTable: "HabitLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}

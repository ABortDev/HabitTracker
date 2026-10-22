using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Habit_Tracker_Project.Migrations
{
    /// <inheritdoc />
    public partial class RemoveHabitLogPointTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HabitReminders_HabitSchedules_HabitScheduleId",
                table: "HabitReminders");

            migrationBuilder.DropForeignKey(
                name: "FK_Habits_AspNetUsers_UserId",
                table: "Habits");

            migrationBuilder.DropForeignKey(
                name: "FK_PointTransactions_HabitLogs_HabitLogId",
                table: "PointTransactions");

            migrationBuilder.DropTable(
                name: "AdminLogs");

            migrationBuilder.DropTable(
                name: "HabitSchedules");

            migrationBuilder.DropIndex(
                name: "IX_PointTransactions_HabitLogId",
                table: "PointTransactions");

            migrationBuilder.DropIndex(
                name: "IX_HabitReminders_HabitScheduleId",
                table: "HabitReminders");

            migrationBuilder.DropIndex(
                name: "IX_HabitLogs_HabitId",
                table: "HabitLogs");

            migrationBuilder.DropCheckConstraint(
                name: "CK_User_PointsBalance_NonNegative",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "Habits");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "Habits");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Habits");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Habits");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Habits");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Habits");

            migrationBuilder.DropColumn(
                name: "HabitScheduleId",
                table: "HabitReminders");

            migrationBuilder.DropColumn(
                name: "LastSentAt",
                table: "HabitReminders");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "HabitReminders");

            migrationBuilder.DropColumn(
                name: "NextTriggerAt",
                table: "HabitReminders");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "HabitLogs");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "HabitLogs");

            migrationBuilder.DropColumn(
                name: "PointsAwarded",
                table: "HabitLogs");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "HabitCategories");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "HabitCategories");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "HabitCategories");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "HabitLogId",
                table: "PointTransactions",
                newName: "HabitId");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "PointTransactions",
                newName: "Points");

            migrationBuilder.RenameColumn(
                name: "TargetUnit",
                table: "Habits",
                newName: "Unit");

            migrationBuilder.RenameColumn(
                name: "Unit",
                table: "HabitLogs",
                newName: "Comment");

            migrationBuilder.RenameColumn(
                name: "PointsBalance",
                table: "AspNetUsers",
                newName: "TotalPoints");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "PointTransactions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<decimal>(
                name: "TargetValue",
                table: "Habits",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Difficulty",
                table: "Habits",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Habits",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "ReminderDate",
                table: "HabitReminders",
                type: "time without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<decimal>(
                name: "ActualValue",
                table: "HabitLogs",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "HabitCategories",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "Achievements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Achievements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    HabitId = table.Column<int>(type: "integer", nullable: true),
                    Title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notifications_Habits_HabitId",
                        column: x => x.HabitId,
                        principalTable: "Habits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "UserAchievements",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    AchievementId = table.Column<int>(type: "integer", nullable: false),
                    AwardedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAchievements", x => new { x.UserId, x.AchievementId });
                    table.ForeignKey(
                        name: "FK_UserAchievements_Achievements_AchievementId",
                        column: x => x.AchievementId,
                        principalTable: "Achievements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAchievements_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_HabitId",
                table: "PointTransactions",
                column: "HabitId");

            migrationBuilder.CreateIndex(
                name: "IX_HabitLogs_HabitId_Date",
                table: "HabitLogs",
                columns: new[] { "HabitId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_HabitId",
                table: "Notifications",
                column: "HabitId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAchievements_AchievementId",
                table: "UserAchievements",
                column: "AchievementId");

            migrationBuilder.AddForeignKey(
                name: "FK_Habits_AspNetUsers_UserId",
                table: "Habits",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PointTransactions_Habits_HabitId",
                table: "PointTransactions",
                column: "HabitId",
                principalTable: "Habits",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Habits_AspNetUsers_UserId",
                table: "Habits");

            migrationBuilder.DropForeignKey(
                name: "FK_PointTransactions_Habits_HabitId",
                table: "PointTransactions");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "UserAchievements");

            migrationBuilder.DropTable(
                name: "Achievements");

            migrationBuilder.DropIndex(
                name: "IX_PointTransactions_HabitId",
                table: "PointTransactions");

            migrationBuilder.DropIndex(
                name: "IX_HabitLogs_HabitId_Date",
                table: "HabitLogs");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "PointTransactions");

            migrationBuilder.DropColumn(
                name: "Difficulty",
                table: "Habits");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Habits");

            migrationBuilder.RenameColumn(
                name: "Points",
                table: "PointTransactions",
                newName: "Amount");

            migrationBuilder.RenameColumn(
                name: "HabitId",
                table: "PointTransactions",
                newName: "HabitLogId");

            migrationBuilder.RenameColumn(
                name: "Unit",
                table: "Habits",
                newName: "TargetUnit");

            migrationBuilder.RenameColumn(
                name: "Comment",
                table: "HabitLogs",
                newName: "Unit");

            migrationBuilder.RenameColumn(
                name: "TotalPoints",
                table: "AspNetUsers",
                newName: "PointsBalance");

            migrationBuilder.AlterColumn<decimal>(
                name: "TargetValue",
                table: "Habits",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Habits",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "Habits",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Habits",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Habits",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Habits",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Habits",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ReminderDate",
                table: "HabitReminders",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone");

            migrationBuilder.AddColumn<int>(
                name: "HabitScheduleId",
                table: "HabitReminders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSentAt",
                table: "HabitReminders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "HabitReminders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextTriggerAt",
                table: "HabitReminders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ActualValue",
                table: "HabitLogs",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "HabitLogs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "HabitLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PointsAwarded",
                table: "HabitLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "HabitCategories",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "HabitCategories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "HabitCategories",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "HabitCategories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AdminLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AdminId = table.Column<string>(type: "text", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    Details = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdminLogs_AspNetUsers_AdminId",
                        column: x => x.AdminId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HabitSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HabitId = table.Column<int>(type: "integer", nullable: false),
                    ScheduleDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HabitSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HabitSchedules_Habits_HabitId",
                        column: x => x.HabitId,
                        principalTable: "Habits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_HabitLogId",
                table: "PointTransactions",
                column: "HabitLogId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HabitReminders_HabitScheduleId",
                table: "HabitReminders",
                column: "HabitScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_HabitLogs_HabitId",
                table: "HabitLogs",
                column: "HabitId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_User_PointsBalance_NonNegative",
                table: "AspNetUsers",
                sql: "\"PointsBalance\" >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_AdminLogs_AdminId",
                table: "AdminLogs",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_HabitSchedules_HabitId",
                table: "HabitSchedules",
                column: "HabitId");

            migrationBuilder.AddForeignKey(
                name: "FK_HabitReminders_HabitSchedules_HabitScheduleId",
                table: "HabitReminders",
                column: "HabitScheduleId",
                principalTable: "HabitSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Habits_AspNetUsers_UserId",
                table: "Habits",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PointTransactions_HabitLogs_HabitLogId",
                table: "PointTransactions",
                column: "HabitLogId",
                principalTable: "HabitLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

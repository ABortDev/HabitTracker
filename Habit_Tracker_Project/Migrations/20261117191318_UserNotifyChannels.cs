using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Habit_Tracker_Project.Migrations
{
    /// <inheritdoc />
    public partial class UserNotifyChannels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PushSubscriptionJson",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TelegramChatId",
                table: "AspNetUsers",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PushSubscriptionJson",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TelegramChatId",
                table: "AspNetUsers");
        }
    }
}

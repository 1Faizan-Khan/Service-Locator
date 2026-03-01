using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceLocator.Migrations
{
    /// <inheritdoc />
    public partial class AddDemoFieldsToNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DemoSessionId",
                table: "Notifications",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsDemo",
                table: "Notifications",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DemoSessionId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "IsDemo",
                table: "Notifications");
        }
    }
}

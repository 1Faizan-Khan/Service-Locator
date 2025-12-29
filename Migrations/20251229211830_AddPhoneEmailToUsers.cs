using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceLocator.Migrations
{
    /// <inheritdoc />
    public partial class AddPhoneEmailToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Provider",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsAccepted",
                table: "Notifications",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Customer",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Provider");

            migrationBuilder.DropColumn(
                name: "IsAccepted",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Customer");
        }
    }
}

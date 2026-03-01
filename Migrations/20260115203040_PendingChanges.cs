using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceLocator.Migrations
{
    /// <inheritdoc />
    public partial class PendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasSeenGuidance",
                table: "Provider",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasSeenGuidance",
                table: "Customer",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasSeenGuidance",
                table: "Provider");

            migrationBuilder.DropColumn(
                name: "HasSeenGuidance",
                table: "Customer");
        }
    }
}

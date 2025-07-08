using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductivityQuestManager.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveBool : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Units",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Units");
        }
    }
}

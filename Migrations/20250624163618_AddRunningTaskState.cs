using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductivityQuestManager.Migrations
{
    /// <inheritdoc />
    public partial class AddRunningTaskState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRunning",
                table: "Tasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAt",
                table: "Tasks",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRunning",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "StartedAt",
                table: "Tasks");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductivityQuestManager.Migrations
{
    /// <inheritdoc />
    public partial class AddRepeatableTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CooldownMinutes",
                table: "Tasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsRepeatable",
                table: "Tasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastCompletedAt",
                table: "Tasks",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CooldownMinutes",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "IsRepeatable",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "LastCompletedAt",
                table: "Tasks");
        }
    }
}

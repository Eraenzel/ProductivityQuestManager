using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductivityQuestManager.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestAndQuestResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuestResult",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    QuestId = table.Column<int>(type: "INTEGER", nullable: false),
                    UnitId = table.Column<int>(type: "INTEGER", nullable: false),
                    WasSuccessful = table.Column<bool>(type: "INTEGER", nullable: false),
                    OutcomeSummary = table.Column<string>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExperienceGained = table.Column<int>(type: "INTEGER", nullable: false),
                    Loot = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestResult", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestResult_Quests_QuestId",
                        column: x => x.QuestId,
                        principalTable: "Quests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestResult_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuestResult_QuestId",
                table: "QuestResult",
                column: "QuestId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestResult_UnitId",
                table: "QuestResult",
                column: "UnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestResult");
        }
    }
}

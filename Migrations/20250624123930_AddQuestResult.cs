using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductivityQuestManager.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuestResult_Quests_QuestId",
                table: "QuestResult");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestResult_Units_UnitId",
                table: "QuestResult");

            migrationBuilder.DropPrimaryKey(
                name: "PK_QuestResult",
                table: "QuestResult");

            migrationBuilder.RenameTable(
                name: "QuestResult",
                newName: "QuestResults");

            migrationBuilder.RenameIndex(
                name: "IX_QuestResult_UnitId",
                table: "QuestResults",
                newName: "IX_QuestResults_UnitId");

            migrationBuilder.RenameIndex(
                name: "IX_QuestResult_QuestId",
                table: "QuestResults",
                newName: "IX_QuestResults_QuestId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuestResults",
                table: "QuestResults",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QuestResults_Quests_QuestId",
                table: "QuestResults",
                column: "QuestId",
                principalTable: "Quests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestResults_Units_UnitId",
                table: "QuestResults",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuestResults_Quests_QuestId",
                table: "QuestResults");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestResults_Units_UnitId",
                table: "QuestResults");

            migrationBuilder.DropPrimaryKey(
                name: "PK_QuestResults",
                table: "QuestResults");

            migrationBuilder.RenameTable(
                name: "QuestResults",
                newName: "QuestResult");

            migrationBuilder.RenameIndex(
                name: "IX_QuestResults_UnitId",
                table: "QuestResult",
                newName: "IX_QuestResult_UnitId");

            migrationBuilder.RenameIndex(
                name: "IX_QuestResults_QuestId",
                table: "QuestResult",
                newName: "IX_QuestResult_QuestId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuestResult",
                table: "QuestResult",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QuestResult_Quests_QuestId",
                table: "QuestResult",
                column: "QuestId",
                principalTable: "Quests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestResult_Units_UnitId",
                table: "QuestResult",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

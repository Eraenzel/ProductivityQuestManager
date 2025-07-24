using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductivityQuestManager.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskModelFKToQuestResults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int?>(
        name: "TaskModelId",
        table: "QuestResults",
        nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestResults_TaskModelId",
                table: "QuestResults",
                column: "TaskModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuestResults_Tasks_TaskModelId",
                table: "QuestResults",
                column: "TaskModelId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("FK_QuestResults_Tasks_TaskModelId", "QuestResults");
            migrationBuilder.DropIndex("IX_QuestResults_TaskModelId", "QuestResults");
            migrationBuilder.DropColumn("TaskModelId", "QuestResults");
        }
    }
}

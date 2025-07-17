using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductivityQuestManager.Migrations
{
    /// <inheritdoc />
    public partial class AddedTagInContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimeEntryTag_Tag_TagId",
                table: "TimeEntryTag");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeEntryTag_TimeEntries_TimeEntryId",
                table: "TimeEntryTag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TimeEntryTag",
                table: "TimeEntryTag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tag",
                table: "Tag");

            migrationBuilder.RenameTable(
                name: "TimeEntryTag",
                newName: "TimeEntryTags");

            migrationBuilder.RenameTable(
                name: "Tag",
                newName: "Tags");

            migrationBuilder.RenameIndex(
                name: "IX_TimeEntryTag_TagId",
                table: "TimeEntryTags",
                newName: "IX_TimeEntryTags_TagId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TimeEntryTags",
                table: "TimeEntryTags",
                columns: new[] { "TimeEntryId", "TagId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tags",
                table: "Tags",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeEntryTags_Tags_TagId",
                table: "TimeEntryTags",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TimeEntryTags_TimeEntries_TimeEntryId",
                table: "TimeEntryTags",
                column: "TimeEntryId",
                principalTable: "TimeEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimeEntryTags_Tags_TagId",
                table: "TimeEntryTags");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeEntryTags_TimeEntries_TimeEntryId",
                table: "TimeEntryTags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TimeEntryTags",
                table: "TimeEntryTags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tags",
                table: "Tags");

            migrationBuilder.RenameTable(
                name: "TimeEntryTags",
                newName: "TimeEntryTag");

            migrationBuilder.RenameTable(
                name: "Tags",
                newName: "Tag");

            migrationBuilder.RenameIndex(
                name: "IX_TimeEntryTags_TagId",
                table: "TimeEntryTag",
                newName: "IX_TimeEntryTag_TagId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TimeEntryTag",
                table: "TimeEntryTag",
                columns: new[] { "TimeEntryId", "TagId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tag",
                table: "Tag",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeEntryTag_Tag_TagId",
                table: "TimeEntryTag",
                column: "TagId",
                principalTable: "Tag",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TimeEntryTag_TimeEntries_TimeEntryId",
                table: "TimeEntryTag",
                column: "TimeEntryId",
                principalTable: "TimeEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

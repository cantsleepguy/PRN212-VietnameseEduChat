using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRN212_VietnameseEduChat.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddSourceDocumentToResearchQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SourceDocumentId",
                table: "ResearchQuestions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResearchQuestions_SourceDocumentId",
                table: "ResearchQuestions",
                column: "SourceDocumentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ResearchQuestions_Documents_SourceDocumentId",
                table: "ResearchQuestions",
                column: "SourceDocumentId",
                principalTable: "Documents",
                principalColumn: "DocumentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ResearchQuestions_Documents_SourceDocumentId",
                table: "ResearchQuestions");

            migrationBuilder.DropIndex(
                name: "IX_ResearchQuestions_SourceDocumentId",
                table: "ResearchQuestions");

            migrationBuilder.DropColumn(
                name: "SourceDocumentId",
                table: "ResearchQuestions");
        }
    }
}

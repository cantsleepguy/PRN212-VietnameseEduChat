using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRN212_VietnameseEduChat.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddResearchDocumentChunks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChunkingStrategyKey",
                table: "ResearchExperiments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "fixed-baseline");

            migrationBuilder.AddColumn<int>(
                name: "EmbeddingDimensions",
                table: "ResearchExperiments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "EmbeddingProvider",
                table: "ResearchExperiments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "OpenAI");

            migrationBuilder.CreateTable(
                name: "ResearchDocumentChunks",
                columns: table => new
                {
                    ResearchDocumentChunkId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentId = table.Column<int>(type: "int", nullable: false),
                    ChunkingStrategyKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ChunkingStrategyName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ChunkSize = table.Column<int>(type: "int", nullable: false),
                    ChunkOverlap = table.Column<int>(type: "int", nullable: false),
                    ChunkIndex = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmbeddingProvider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EmbeddingModelName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    EmbeddingDimensions = table.Column<int>(type: "int", nullable: false),
                    EmbeddingJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResearchDocumentChunks", x => x.ResearchDocumentChunkId);
                    table.ForeignKey(
                        name: "FK_ResearchDocumentChunks_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "DocumentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResearchDocumentChunks_DocumentId_ChunkingStrategyKey_EmbeddingModelName_ChunkIndex",
                table: "ResearchDocumentChunks",
                columns: new[] { "DocumentId", "ChunkingStrategyKey", "EmbeddingModelName", "ChunkIndex" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResearchDocumentChunks");

            migrationBuilder.DropColumn(
                name: "ChunkingStrategyKey",
                table: "ResearchExperiments");

            migrationBuilder.DropColumn(
                name: "EmbeddingDimensions",
                table: "ResearchExperiments");

            migrationBuilder.DropColumn(
                name: "EmbeddingProvider",
                table: "ResearchExperiments");
        }
    }
}

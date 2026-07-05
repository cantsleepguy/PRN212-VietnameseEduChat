using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRN212_VietnameseEduChat.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddResearchBenchmarkTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ResearchExperiments",
                columns: table => new
                {
                    ResearchExperimentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExperimentName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ExperimentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AnswerModelName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmbeddingModelName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChunkingStrategyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChunkSize = table.Column<int>(type: "int", nullable: false),
                    ChunkOverlap = table.Column<int>(type: "int", nullable: false),
                    TopK = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FinishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResearchExperiments", x => x.ResearchExperimentId);
                });

            migrationBuilder.CreateTable(
                name: "ResearchQuestions",
                columns: table => new
                {
                    ResearchQuestionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubjectId = table.Column<int>(type: "int", nullable: true),
                    ChapterId = table.Column<int>(type: "int", nullable: true),
                    Question = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GroundTruthAnswer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpectedKeywords = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpectedSource = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResearchQuestions", x => x.ResearchQuestionId);
                    table.ForeignKey(
                        name: "FK_ResearchQuestions_Chapters_ChapterId",
                        column: x => x.ChapterId,
                        principalTable: "Chapters",
                        principalColumn: "ChapterId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResearchQuestions_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "SubjectId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ResearchResults",
                columns: table => new
                {
                    ResearchResultId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResearchExperimentId = table.Column<int>(type: "int", nullable: false),
                    ResearchQuestionId = table.Column<int>(type: "int", nullable: false),
                    GeneratedAnswer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RetrievedContext = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RetrievedSourcesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnswerSimilarityScore = table.Column<double>(type: "float", nullable: false),
                    ContextRelevanceScore = table.Column<double>(type: "float", nullable: false),
                    GroundednessScore = table.Column<double>(type: "float", nullable: false),
                    KeywordHitScore = table.Column<double>(type: "float", nullable: false),
                    OverallScore = table.Column<double>(type: "float", nullable: false),
                    LatencyMs = table.Column<long>(type: "bigint", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResearchResults", x => x.ResearchResultId);
                    table.ForeignKey(
                        name: "FK_ResearchResults_ResearchExperiments_ResearchExperimentId",
                        column: x => x.ResearchExperimentId,
                        principalTable: "ResearchExperiments",
                        principalColumn: "ResearchExperimentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResearchResults_ResearchQuestions_ResearchQuestionId",
                        column: x => x.ResearchQuestionId,
                        principalTable: "ResearchQuestions",
                        principalColumn: "ResearchQuestionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResearchQuestions_ChapterId",
                table: "ResearchQuestions",
                column: "ChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchQuestions_SubjectId",
                table: "ResearchQuestions",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchResults_ResearchExperimentId",
                table: "ResearchResults",
                column: "ResearchExperimentId");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchResults_ResearchQuestionId",
                table: "ResearchResults",
                column: "ResearchQuestionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResearchResults");

            migrationBuilder.DropTable(
                name: "ResearchExperiments");

            migrationBuilder.DropTable(
                name: "ResearchQuestions");
        }
    }
}

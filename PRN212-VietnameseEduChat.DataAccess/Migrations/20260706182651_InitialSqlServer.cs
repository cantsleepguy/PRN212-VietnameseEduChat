using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRN212_VietnameseEduChat.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialSqlServer : Migration
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
                    EmbeddingProvider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "OpenAI"),
                    EmbeddingModelName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmbeddingDimensions = table.Column<int>(type: "int", nullable: false),
                    ChunkingStrategyKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "fixed-baseline"),
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
                name: "Roles",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "Subjects",
                columns: table => new
                {
                    SubjectId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubjectName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects", x => x.SubjectId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chapters",
                columns: table => new
                {
                    ChapterId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    ChapterName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chapters", x => x.ChapterId);
                    table.ForeignKey(
                        name: "FK_Chapters_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "SubjectId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChatSessions",
                columns: table => new
                {
                    ChatSessionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SubjectId = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatSessions", x => x.ChatSessionId);
                    table.ForeignKey(
                        name: "FK_ChatSessions_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "SubjectId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ChatSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubjectLecturers",
                columns: table => new
                {
                    SubjectLecturerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    LecturerId = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedBy = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectLecturers", x => x.SubjectLecturerId);
                    table.ForeignKey(
                        name: "FK_SubjectLecturers_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "SubjectId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubjectLecturers_Users_AssignedBy",
                        column: x => x.AssignedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubjectLecturers_Users_LecturerId",
                        column: x => x.LecturerId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    DocumentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StoredFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadedBy = table.Column<int>(type: "int", nullable: false),
                    SubjectId = table.Column<int>(type: "int", nullable: true),
                    ChapterId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TotalChunks = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReviewedBy = table.Column<int>(type: "int", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.DocumentId);
                    table.ForeignKey(
                        name: "FK_Documents_Chapters_ChapterId",
                        column: x => x.ChapterId,
                        principalTable: "Chapters",
                        principalColumn: "ChapterId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "SubjectId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_Users_ReviewedBy",
                        column: x => x.ReviewedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_Users_UploadedBy",
                        column: x => x.UploadedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    ChatMessageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChatSessionId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.ChatMessageId);
                    table.ForeignKey(
                        name: "FK_ChatMessages_ChatSessions_ChatSessionId",
                        column: x => x.ChatSessionId,
                        principalTable: "ChatSessions",
                        principalColumn: "ChatSessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentChunks",
                columns: table => new
                {
                    DocumentChunkId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentId = table.Column<int>(type: "int", nullable: false),
                    ChunkIndex = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmbeddingJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmbeddingModel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EmbeddingDimensions = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentChunks", x => x.DocumentChunkId);
                    table.ForeignKey(
                        name: "FK_DocumentChunks_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "DocumentId",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateTable(
                name: "ResearchQuestions",
                columns: table => new
                {
                    ResearchQuestionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubjectId = table.Column<int>(type: "int", nullable: true),
                    ChapterId = table.Column<int>(type: "int", nullable: true),
                    SourceDocumentId = table.Column<int>(type: "int", nullable: true),
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
                        name: "FK_ResearchQuestions_Documents_SourceDocumentId",
                        column: x => x.SourceDocumentId,
                        principalTable: "Documents",
                        principalColumn: "DocumentId");
                    table.ForeignKey(
                        name: "FK_ResearchQuestions_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "SubjectId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessageSources",
                columns: table => new
                {
                    ChatMessageSourceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChatMessageId = table.Column<int>(type: "int", nullable: false),
                    DocumentChunkId = table.Column<int>(type: "int", nullable: false),
                    SimilarityScore = table.Column<double>(type: "float", nullable: false),
                    Excerpt = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessageSources", x => x.ChatMessageSourceId);
                    table.ForeignKey(
                        name: "FK_ChatMessageSources_ChatMessages_ChatMessageId",
                        column: x => x.ChatMessageId,
                        principalTable: "ChatMessages",
                        principalColumn: "ChatMessageId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChatMessageSources_DocumentChunks_DocumentChunkId",
                        column: x => x.DocumentChunkId,
                        principalTable: "DocumentChunks",
                        principalColumn: "DocumentChunkId",
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
                name: "IX_Chapters_SubjectId",
                table: "Chapters",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ChatSessionId",
                table: "ChatMessages",
                column: "ChatSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessageSources_ChatMessageId",
                table: "ChatMessageSources",
                column: "ChatMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessageSources_DocumentChunkId",
                table: "ChatMessageSources",
                column: "DocumentChunkId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessions_SubjectId",
                table: "ChatSessions",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessions_UserId",
                table: "ChatSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentChunks_DocumentId",
                table: "DocumentChunks",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ChapterId",
                table: "Documents",
                column: "ChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ReviewedBy",
                table: "Documents",
                column: "ReviewedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_SubjectId",
                table: "Documents",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_UploadedBy",
                table: "Documents",
                column: "UploadedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchDocumentChunks_DocumentId_ChunkingStrategyKey_EmbeddingModelName_ChunkIndex",
                table: "ResearchDocumentChunks",
                columns: new[] { "DocumentId", "ChunkingStrategyKey", "EmbeddingModelName", "ChunkIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResearchQuestions_ChapterId",
                table: "ResearchQuestions",
                column: "ChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchQuestions_SourceDocumentId",
                table: "ResearchQuestions",
                column: "SourceDocumentId");

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

            migrationBuilder.CreateIndex(
                name: "IX_SubjectLecturers_AssignedBy",
                table: "SubjectLecturers",
                column: "AssignedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectLecturers_LecturerId",
                table: "SubjectLecturers",
                column: "LecturerId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectLecturers_SubjectId_LecturerId",
                table: "SubjectLecturers",
                columns: new[] { "SubjectId", "LecturerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatMessageSources");

            migrationBuilder.DropTable(
                name: "ResearchDocumentChunks");

            migrationBuilder.DropTable(
                name: "ResearchResults");

            migrationBuilder.DropTable(
                name: "SubjectLecturers");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "DocumentChunks");

            migrationBuilder.DropTable(
                name: "ResearchExperiments");

            migrationBuilder.DropTable(
                name: "ResearchQuestions");

            migrationBuilder.DropTable(
                name: "ChatSessions");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "Chapters");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Subjects");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}

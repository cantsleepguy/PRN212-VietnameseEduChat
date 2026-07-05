using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRN212_VietnameseEduChat.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddSubjectLecturerAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubjectLecturers");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRN212_VietnameseEduChat.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentApprovalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Documents",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "Documents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewedBy",
                table: "Documents",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ReviewedBy",
                table: "Documents",
                column: "ReviewedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Users_ReviewedBy",
                table: "Documents",
                column: "ReviewedBy",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Users_ReviewedBy",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_ReviewedBy",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "ReviewedBy",
                table: "Documents");
        }
    }
}

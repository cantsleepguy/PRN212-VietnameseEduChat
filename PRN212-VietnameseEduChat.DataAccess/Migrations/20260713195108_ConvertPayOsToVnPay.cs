using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRN212_VietnameseEduChat.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ConvertPayOsToVnPay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TargetStartDate",
                table: "Payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VnPayBankCode",
                table: "Payments",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VnPayCardType",
                table: "Payments",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VnPayPayDate",
                table: "Payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VnPayResponseCode",
                table: "Payments",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VnPayTransactionStatus",
                table: "Payments",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.Sql(
    """
    UPDATE Payments
    SET
        Status = 'Failed',
        FailureReason =
            'Giao dịch payOS cũ đã bị vô hiệu hóa sau khi chuyển sang VNPay Sandbox.'
    WHERE Provider = 'PayOS'
      AND Status = 'Pending';
    """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TargetStartDate",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "VnPayBankCode",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "VnPayCardType",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "VnPayPayDate",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "VnPayResponseCode",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "VnPayTransactionStatus",
                table: "Payments");
        }
    }
}

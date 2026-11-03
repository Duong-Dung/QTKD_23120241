using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QTKD.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddContractFieldsForWeek06 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttachmentUrl",
                table: "Contracts",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedAt",
                table: "Contracts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Terms",
                table: "Contracts",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "Contracts",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "Contracts",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AttachmentUrl", "SubmittedAt", "Terms", "TotalAmount" },
                values: new object[] { "https://storage.qtkd.vn/contracts/HD-2026-018.pdf", null, "Cung cấp giải pháp phần mềm Quản lý Kinh doanh trọn gói", 50000000m });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentUrl",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "Terms",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "Contracts");
        }
    }
}

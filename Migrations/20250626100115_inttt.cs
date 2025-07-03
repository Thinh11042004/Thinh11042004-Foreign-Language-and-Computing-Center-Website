using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Migrations
{
    /// <inheritdoc />
    public partial class inttt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 26, 10, 1, 14, 26, DateTimeKind.Utc).AddTicks(1595));

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 26, 10, 1, 14, 26, DateTimeKind.Utc).AddTicks(1600));

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 26, 10, 1, 14, 26, DateTimeKind.Utc).AddTicks(1603));

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "4",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 26, 10, 1, 14, 26, DateTimeKind.Utc).AddTicks(1605));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 26, 4, 14, 10, 708, DateTimeKind.Utc).AddTicks(8459));

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 26, 4, 14, 10, 708, DateTimeKind.Utc).AddTicks(8472));

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 26, 4, 14, 10, 708, DateTimeKind.Utc).AddTicks(8479));

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "4",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 26, 4, 14, 10, 708, DateTimeKind.Utc).AddTicks(8485));
        }
    }
}

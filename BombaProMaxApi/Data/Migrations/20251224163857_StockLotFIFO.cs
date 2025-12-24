using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BombaProMaxApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class StockLotFIFO : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 24, 16, 38, 56, 100, DateTimeKind.Utc).AddTicks(2650));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 24, 16, 38, 56, 100, DateTimeKind.Utc).AddTicks(2663));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 24, 15, 29, 23, 822, DateTimeKind.Utc).AddTicks(7476));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 24, 15, 29, 23, 822, DateTimeKind.Utc).AddTicks(7488));
        }
    }
}

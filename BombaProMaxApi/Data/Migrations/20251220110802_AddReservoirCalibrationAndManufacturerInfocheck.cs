using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BombaProMaxApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReservoirCalibrationAndManufacturerInfocheck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 20, 11, 8, 1, 503, DateTimeKind.Utc).AddTicks(4028));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 20, 11, 8, 1, 503, DateTimeKind.Utc).AddTicks(4041));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 20, 11, 7, 16, 65, DateTimeKind.Utc).AddTicks(5871));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 20, 11, 7, 16, 65, DateTimeKind.Utc).AddTicks(5882));
        }
    }
}

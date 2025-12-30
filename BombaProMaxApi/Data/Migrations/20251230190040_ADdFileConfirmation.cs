using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BombaProMaxApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class ADdFileConfirmation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PieceJustificativeBase64",
                table: "DepotsCaisse",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PieceJustificativeNom",
                table: "DepotsCaisse",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PieceJustificativeType",
                table: "DepotsCaisse",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 30, 19, 0, 39, 290, DateTimeKind.Utc).AddTicks(469));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 30, 19, 0, 39, 290, DateTimeKind.Utc).AddTicks(482));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PieceJustificativeBase64",
                table: "DepotsCaisse");

            migrationBuilder.DropColumn(
                name: "PieceJustificativeNom",
                table: "DepotsCaisse");

            migrationBuilder.DropColumn(
                name: "PieceJustificativeType",
                table: "DepotsCaisse");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 30, 18, 36, 58, 407, DateTimeKind.Utc).AddTicks(841));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 30, 18, 36, 58, 407, DateTimeKind.Utc).AddTicks(856));
        }
    }
}

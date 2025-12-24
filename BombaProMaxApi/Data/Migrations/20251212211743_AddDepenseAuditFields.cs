using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BombaProMaxApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDepenseAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreePar",
                table: "Depenses",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreation",
                table: "Depenses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateModification",
                table: "Depenses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Depenses",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiePar",
                table: "Depenses",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 12, 21, 17, 42, 58, DateTimeKind.Utc).AddTicks(459));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 12, 21, 17, 42, 58, DateTimeKind.Utc).AddTicks(472));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreePar",
                table: "Depenses");

            migrationBuilder.DropColumn(
                name: "DateCreation",
                table: "Depenses");

            migrationBuilder.DropColumn(
                name: "DateModification",
                table: "Depenses");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Depenses");

            migrationBuilder.DropColumn(
                name: "ModifiePar",
                table: "Depenses");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 12, 7, 59, 25, 543, DateTimeKind.Utc).AddTicks(1620));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 12, 7, 59, 25, 543, DateTimeKind.Utc).AddTicks(1637));
        }
    }
}

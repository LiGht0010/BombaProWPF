using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BombaProMaxApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AjouteParModifierParMods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pompes_Reservoirs_ReservoirAssocieID",
                table: "Pompes");

            migrationBuilder.AlterColumn<int>(
                name: "ReservoirAssocieID",
                table: "Pompes",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "AjoutePar",
                table: "Fournisseurs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreation",
                table: "Fournisseurs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateModification",
                table: "Fournisseurs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModifiePar",
                table: "Fournisseurs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AjoutePar",
                table: "Achats",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreation",
                table: "Achats",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateModification",
                table: "Achats",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModifiePar",
                table: "Achats",
                type: "integer",
                nullable: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_Pompes_Reservoirs_ReservoirAssocieID",
                table: "Pompes",
                column: "ReservoirAssocieID",
                principalTable: "Reservoirs",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pompes_Reservoirs_ReservoirAssocieID",
                table: "Pompes");

            migrationBuilder.DropColumn(
                name: "AjoutePar",
                table: "Fournisseurs");

            migrationBuilder.DropColumn(
                name: "DateCreation",
                table: "Fournisseurs");

            migrationBuilder.DropColumn(
                name: "DateModification",
                table: "Fournisseurs");

            migrationBuilder.DropColumn(
                name: "ModifiePar",
                table: "Fournisseurs");

            migrationBuilder.DropColumn(
                name: "AjoutePar",
                table: "Achats");

            migrationBuilder.DropColumn(
                name: "DateCreation",
                table: "Achats");

            migrationBuilder.DropColumn(
                name: "DateModification",
                table: "Achats");

            migrationBuilder.DropColumn(
                name: "ModifiePar",
                table: "Achats");

            migrationBuilder.AlterColumn<int>(
                name: "ReservoirAssocieID",
                table: "Pompes",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2025, 11, 23, 15, 36, 36, 720, DateTimeKind.Utc).AddTicks(8222));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2025, 11, 23, 15, 36, 36, 720, DateTimeKind.Utc).AddTicks(8234));

            migrationBuilder.AddForeignKey(
                name: "FK_Pompes_Reservoirs_ReservoirAssocieID",
                table: "Pompes",
                column: "ReservoirAssocieID",
                principalTable: "Reservoirs",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BombaProMaxApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPeriodeIdToCreditTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PeriodeID",
                table: "CreditTransactions",
                type: "integer",
                nullable: true);

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "ID", "Nom" },
                values: new object[,]
                {
                    { 1, "CARBURANT" },
                    { 2, "LUBRIFIANT" },
                    { 3, "ARTICLE" }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 28, 15, 24, 56, 729, DateTimeKind.Utc).AddTicks(7006));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 28, 15, 24, 56, 729, DateTimeKind.Utc).AddTicks(7193));

            migrationBuilder.CreateIndex(
                name: "IX_CreditTransactions_PeriodeID",
                table: "CreditTransactions",
                column: "PeriodeID");

            migrationBuilder.AddForeignKey(
                name: "FK_CreditTransactions_Periodes_PeriodeID",
                table: "CreditTransactions",
                column: "PeriodeID",
                principalTable: "Periodes",
                principalColumn: "PeriodeID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CreditTransactions_Periodes_PeriodeID",
                table: "CreditTransactions");

            migrationBuilder.DropIndex(
                name: "IX_CreditTransactions_PeriodeID",
                table: "CreditTransactions");

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "ID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "ID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "ID",
                keyValue: 3);

            migrationBuilder.DropColumn(
                name: "PeriodeID",
                table: "CreditTransactions");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 27, 16, 1, 54, 647, DateTimeKind.Utc).AddTicks(5890));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 27, 16, 1, 54, 647, DateTimeKind.Utc).AddTicks(5904));
        }
    }
}

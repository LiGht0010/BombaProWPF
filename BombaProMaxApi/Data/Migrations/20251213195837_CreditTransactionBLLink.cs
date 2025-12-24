using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BombaProMaxApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreditTransactionBLLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BonLivraisonID",
                table: "CreditTransactions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EstEnBL",
                table: "CreditTransactions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 13, 19, 58, 35, 930, DateTimeKind.Utc).AddTicks(8203));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 13, 19, 58, 35, 930, DateTimeKind.Utc).AddTicks(8216));

            migrationBuilder.CreateIndex(
                name: "IX_CreditTransactions_BonLivraisonID",
                table: "CreditTransactions",
                column: "BonLivraisonID");

            migrationBuilder.AddForeignKey(
                name: "FK_CreditTransactions_BonsLivraison_BonLivraisonID",
                table: "CreditTransactions",
                column: "BonLivraisonID",
                principalTable: "BonsLivraison",
                principalColumn: "ID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CreditTransactions_BonsLivraison_BonLivraisonID",
                table: "CreditTransactions");

            migrationBuilder.DropIndex(
                name: "IX_CreditTransactions_BonLivraisonID",
                table: "CreditTransactions");

            migrationBuilder.DropColumn(
                name: "BonLivraisonID",
                table: "CreditTransactions");

            migrationBuilder.DropColumn(
                name: "EstEnBL",
                table: "CreditTransactions");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 13, 17, 51, 25, 52, DateTimeKind.Utc).AddTicks(3799));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 13, 17, 51, 25, 52, DateTimeKind.Utc).AddTicks(3811));
        }
    }
}

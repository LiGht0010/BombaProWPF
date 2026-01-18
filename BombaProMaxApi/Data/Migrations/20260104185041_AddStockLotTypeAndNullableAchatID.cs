using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BombaProMaxApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStockLotTypeAndNullableAchatID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "AchatID",
                table: "StockLots",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "StockLots",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "StockLots",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 4, 18, 50, 40, 378, DateTimeKind.Utc).AddTicks(6701));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 4, 18, 50, 40, 378, DateTimeKind.Utc).AddTicks(6716));

            migrationBuilder.CreateIndex(
                name: "IX_StockLots_ReservoirID_Type",
                table: "StockLots",
                columns: new[] { "ReservoirID", "Type" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_StockLots_AchatID_Type_Consistency",
                table: "StockLots",
                sql: "(\"Type\" = 1 AND \"AchatID\" IS NOT NULL) OR (\"Type\" != 1 AND \"AchatID\" IS NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_StockLots_PrixAchat_NonNegative",
                table: "StockLots",
                sql: "\"PrixAchat\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_StockLots_QuantiteDisponible_Bounds",
                table: "StockLots",
                sql: "\"QuantiteDisponible\" >= 0 AND \"QuantiteDisponible\" <= \"QuantiteInitiale\"");

            migrationBuilder.AddCheckConstraint(
                name: "CK_StockLots_QuantiteInitiale_Positive",
                table: "StockLots",
                sql: "\"QuantiteInitiale\" > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StockLots_ReservoirID_Type",
                table: "StockLots");

            migrationBuilder.DropCheckConstraint(
                name: "CK_StockLots_AchatID_Type_Consistency",
                table: "StockLots");

            migrationBuilder.DropCheckConstraint(
                name: "CK_StockLots_PrixAchat_NonNegative",
                table: "StockLots");

            migrationBuilder.DropCheckConstraint(
                name: "CK_StockLots_QuantiteDisponible_Bounds",
                table: "StockLots");

            migrationBuilder.DropCheckConstraint(
                name: "CK_StockLots_QuantiteInitiale_Positive",
                table: "StockLots");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "StockLots");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "StockLots");

            migrationBuilder.AlterColumn<int>(
                name: "AchatID",
                table: "StockLots",
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
                value: new DateTime(2025, 12, 31, 22, 2, 35, 419, DateTimeKind.Utc).AddTicks(8134));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 31, 22, 2, 35, 419, DateTimeKind.Utc).AddTicks(8153));
        }
    }
}

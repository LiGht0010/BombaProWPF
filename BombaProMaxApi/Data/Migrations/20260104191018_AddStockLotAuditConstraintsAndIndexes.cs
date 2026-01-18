using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BombaProMaxApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStockLotAuditConstraintsAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StockLots_ReservoirID_DateEntree",
                table: "StockLots");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 4, 19, 10, 17, 229, DateTimeKind.Utc).AddTicks(1943));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 4, 19, 10, 17, 229, DateTimeKind.Utc).AddTicks(1953));

            migrationBuilder.CreateIndex(
                name: "IX_StockLots_ReservoirID_DateEntree_ID",
                table: "StockLots",
                columns: new[] { "ReservoirID", "DateEntree", "ID" });

            // Index for consumption queries by StockLotID (audit trail)
            // Note: This index may already exist from FK creation - create only if not exists
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_StockLotConsumptions_StockLotID""
                ON ""StockLotConsumptions"" (""StockLotID"");
            ");

            migrationBuilder.AddCheckConstraint(
                name: "CK_StockLotConsumptions_PrixUnitaire_NonNegative",
                table: "StockLotConsumptions",
                sql: "\"PrixUnitaire\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_StockLotConsumptions_QuantiteConsommee_Positive",
                table: "StockLotConsumptions",
                sql: "\"QuantiteConsommee\" > 0");

            // ─────────────────────────────────────────────────────────────────
            // PARTIAL UNIQUE INDEX: Enforce only one OpeningBalance per Reservoir
            // Type = 0 is OpeningBalance. This constraint ensures a reservoir
            // cannot have multiple opening balance stock lots.
            // ─────────────────────────────────────────────────────────────────
            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_StockLots_ReservoirID_OpeningBalance_Unique""
                ON ""StockLots"" (""ReservoirID"")
                WHERE ""Type"" = 0 AND ""Statut"" != 'Annulé';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the partial unique index
            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS ""IX_StockLots_ReservoirID_OpeningBalance_Unique"";
            ");

            migrationBuilder.DropIndex(
                name: "IX_StockLots_ReservoirID_DateEntree_ID",
                table: "StockLots");

            // Note: We don't drop IX_StockLotConsumptions_StockLotID as it may have
            // been created by EF for the FK relationship originally

            migrationBuilder.DropCheckConstraint(
                name: "CK_StockLotConsumptions_PrixUnitaire_NonNegative",
                table: "StockLotConsumptions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_StockLotConsumptions_QuantiteConsommee_Positive",
                table: "StockLotConsumptions");

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
                name: "IX_StockLots_ReservoirID_DateEntree",
                table: "StockLots",
                columns: new[] { "ReservoirID", "DateEntree" });
        }
    }
}

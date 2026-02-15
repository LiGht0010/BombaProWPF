using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BombaProMaxApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStockWithdrawals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockWithdrawals",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReservoirID = table.Column<int>(type: "integer", nullable: false),
                    ProduitID = table.Column<int>(type: "integer", nullable: false),
                    Quantite = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    Motif = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UtilisateurID = table.Column<int>(type: "integer", nullable: true),
                    UtilisateurNom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DateRetrait = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NiveauAvant = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    NiveauApres = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    LotsAffectesJson = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockWithdrawals", x => x.ID);
                    table.ForeignKey(
                        name: "FK_StockWithdrawals_Produits_ProduitID",
                        column: x => x.ProduitID,
                        principalTable: "Produits",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StockWithdrawals_Reservoirs_ReservoirID",
                        column: x => x.ReservoirID,
                        principalTable: "Reservoirs",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2026, 2, 4, 18, 5, 40, 292, DateTimeKind.Utc).AddTicks(6585));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2026, 2, 4, 18, 5, 40, 292, DateTimeKind.Utc).AddTicks(6598));

            migrationBuilder.CreateIndex(
                name: "IX_StockWithdrawals_ProduitID",
                table: "StockWithdrawals",
                column: "ProduitID");

            migrationBuilder.CreateIndex(
                name: "IX_StockWithdrawals_ReservoirID",
                table: "StockWithdrawals",
                column: "ReservoirID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockWithdrawals");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2026, 2, 3, 16, 57, 17, 861, DateTimeKind.Utc).AddTicks(1138));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2026, 2, 3, 16, 57, 17, 861, DateTimeKind.Utc).AddTicks(1149));
        }
    }
}

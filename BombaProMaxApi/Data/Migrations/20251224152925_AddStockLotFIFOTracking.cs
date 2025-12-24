using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BombaProMaxApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStockLotFIFOTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockLots",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AchatID = table.Column<int>(type: "integer", nullable: false),
                    ReservoirID = table.Column<int>(type: "integer", nullable: false),
                    ProduitID = table.Column<int>(type: "integer", nullable: false),
                    QuantiteInitiale = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    QuantiteDisponible = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    PrixAchat = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    DateEntree = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Statut = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockLots", x => x.ID);
                    table.ForeignKey(
                        name: "FK_StockLots_Achats_AchatID",
                        column: x => x.AchatID,
                        principalTable: "Achats",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockLots_Produits_ProduitID",
                        column: x => x.ProduitID,
                        principalTable: "Produits",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockLots_Reservoirs_ReservoirID",
                        column: x => x.ReservoirID,
                        principalTable: "Reservoirs",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockLotConsumptions",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StockLotID = table.Column<int>(type: "integer", nullable: false),
                    PeriodeDetailID = table.Column<int>(type: "integer", nullable: false),
                    QuantiteConsommee = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    PrixUnitaire = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    DateConsommation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockLotConsumptions", x => x.ID);
                    table.ForeignKey(
                        name: "FK_StockLotConsumptions_PeriodeDetails_PeriodeDetailID",
                        column: x => x.PeriodeDetailID,
                        principalTable: "PeriodeDetails",
                        principalColumn: "PeriodeDetailID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StockLotConsumptions_StockLots_StockLotID",
                        column: x => x.StockLotID,
                        principalTable: "StockLots",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_StockLotConsumptions_PeriodeDetailID",
                table: "StockLotConsumptions",
                column: "PeriodeDetailID");

            migrationBuilder.CreateIndex(
                name: "IX_StockLotConsumptions_StockLotID",
                table: "StockLotConsumptions",
                column: "StockLotID");

            migrationBuilder.CreateIndex(
                name: "IX_StockLots_AchatID",
                table: "StockLots",
                column: "AchatID");

            migrationBuilder.CreateIndex(
                name: "IX_StockLots_ProduitID",
                table: "StockLots",
                column: "ProduitID");

            migrationBuilder.CreateIndex(
                name: "IX_StockLots_ReservoirID_DateEntree",
                table: "StockLots",
                columns: new[] { "ReservoirID", "DateEntree" });

            migrationBuilder.CreateIndex(
                name: "IX_StockLots_ReservoirID_Statut",
                table: "StockLots",
                columns: new[] { "ReservoirID", "Statut" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockLotConsumptions");

            migrationBuilder.DropTable(
                name: "StockLots");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 21, 20, 33, 27, 879, DateTimeKind.Utc).AddTicks(3937));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 21, 20, 33, 27, 879, DateTimeKind.Utc).AddTicks(3949));
        }
    }
}

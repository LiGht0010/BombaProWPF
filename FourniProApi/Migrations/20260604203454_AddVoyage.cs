using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FourniProApi.Migrations
{
    /// <inheritdoc />
    public partial class AddVoyage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Voyages",
                columns: table => new
                {
                    VoyageId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CamionId = table.Column<int>(type: "integer", nullable: true),
                    ChauffeurId = table.Column<int>(type: "integer", nullable: true),
                    CiterneId = table.Column<int>(type: "integer", nullable: true),
                    DateDepart = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateFinal = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LieuDepart = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LieuTerminal = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    KilometrageDepart = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    KilometrageFinal = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    Statut = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AjoutePar = table.Column<int>(type: "integer", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiePar = table.Column<int>(type: "integer", nullable: true),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Voyages", x => x.VoyageId);
                    table.ForeignKey(
                        name: "FK_Voyages_Camions_CamionId",
                        column: x => x.CamionId,
                        principalTable: "Camions",
                        principalColumn: "CamionId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Voyages_Chauffeurs_ChauffeurId",
                        column: x => x.ChauffeurId,
                        principalTable: "Chauffeurs",
                        principalColumn: "ChauffeurId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Voyages_Citernes_CiterneId",
                        column: x => x.CiterneId,
                        principalTable: "Citernes",
                        principalColumn: "CiterneId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "FraisVoyages",
                columns: table => new
                {
                    FraisVoyageId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VoyageId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Montant = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FraisVoyages", x => x.FraisVoyageId);
                    table.ForeignKey(
                        name: "FK_FraisVoyages_Voyages_VoyageId",
                        column: x => x.VoyageId,
                        principalTable: "Voyages",
                        principalColumn: "VoyageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockVoyages",
                columns: table => new
                {
                    StockVoyageId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VoyageId = table.Column<int>(type: "integer", nullable: false),
                    ProduitId = table.Column<int>(type: "integer", nullable: true),
                    Quantite = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockVoyages", x => x.StockVoyageId);
                    table.ForeignKey(
                        name: "FK_StockVoyages_Produits_ProduitId",
                        column: x => x.ProduitId,
                        principalTable: "Produits",
                        principalColumn: "ProduitId");
                    table.ForeignKey(
                        name: "FK_StockVoyages_Voyages_VoyageId",
                        column: x => x.VoyageId,
                        principalTable: "Voyages",
                        principalColumn: "VoyageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FraisVoyages_VoyageId",
                table: "FraisVoyages",
                column: "VoyageId");

            migrationBuilder.CreateIndex(
                name: "IX_StockVoyages_ProduitId",
                table: "StockVoyages",
                column: "ProduitId");

            migrationBuilder.CreateIndex(
                name: "IX_StockVoyages_VoyageId",
                table: "StockVoyages",
                column: "VoyageId");

            migrationBuilder.CreateIndex(
                name: "IX_Voyages_CamionId",
                table: "Voyages",
                column: "CamionId");

            migrationBuilder.CreateIndex(
                name: "IX_Voyages_ChauffeurId",
                table: "Voyages",
                column: "ChauffeurId");

            migrationBuilder.CreateIndex(
                name: "IX_Voyages_CiterneId",
                table: "Voyages",
                column: "CiterneId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FraisVoyages");

            migrationBuilder.DropTable(
                name: "StockVoyages");

            migrationBuilder.DropTable(
                name: "Voyages");
        }
    }
}

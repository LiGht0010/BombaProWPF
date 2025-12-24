using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BombaProMaxApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class FactureBonLivraison : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BonsLivraison",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NumeroBL = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DateBL = table.Column<DateOnly>(type: "date", nullable: false),
                    ClientID = table.Column<int>(type: "integer", nullable: false),
                    MontantTotal = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    EstFacture = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    AjoutePar = table.Column<int>(type: "integer", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiePar = table.Column<int>(type: "integer", nullable: true),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BonsLivraison", x => x.ID);
                    table.ForeignKey(
                        name: "FK_BonsLivraison_Clients_ClientID",
                        column: x => x.ClientID,
                        principalTable: "Clients",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BonLivraisonDetails",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BonLivraisonID = table.Column<int>(type: "integer", nullable: false),
                    ProduitID = table.Column<int>(type: "integer", nullable: true),
                    ServiceID = table.Column<int>(type: "integer", nullable: true),
                    Quantite = table.Column<int>(type: "integer", nullable: false),
                    PrixUnitaire = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    MontantLigne = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BonLivraisonDetails", x => x.ID);
                    table.ForeignKey(
                        name: "FK_BonLivraisonDetails_BonsLivraison_BonLivraisonID",
                        column: x => x.BonLivraisonID,
                        principalTable: "BonsLivraison",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BonLivraisonDetails_Produits_ProduitID",
                        column: x => x.ProduitID,
                        principalTable: "Produits",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BonLivraisonDetails_Services_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "Services",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FactureBonLivraisons",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FactureID = table.Column<int>(type: "integer", nullable: false),
                    BonLivraisonID = table.Column<int>(type: "integer", nullable: false),
                    DateAssociation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactureBonLivraisons", x => x.ID);
                    table.ForeignKey(
                        name: "FK_FactureBonLivraisons_BonsLivraison_BonLivraisonID",
                        column: x => x.BonLivraisonID,
                        principalTable: "BonsLivraison",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FactureBonLivraisons_Factures_FactureID",
                        column: x => x.FactureID,
                        principalTable: "Factures",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_BonLivraisonDetails_BonLivraisonID",
                table: "BonLivraisonDetails",
                column: "BonLivraisonID");

            migrationBuilder.CreateIndex(
                name: "IX_BonLivraisonDetails_ProduitID",
                table: "BonLivraisonDetails",
                column: "ProduitID");

            migrationBuilder.CreateIndex(
                name: "IX_BonLivraisonDetails_ServiceID",
                table: "BonLivraisonDetails",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_BonsLivraison_ClientID",
                table: "BonsLivraison",
                column: "ClientID");

            migrationBuilder.CreateIndex(
                name: "IX_BonsLivraison_NumeroBL",
                table: "BonsLivraison",
                column: "NumeroBL",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FactureBonLivraisons_BonLivraisonID",
                table: "FactureBonLivraisons",
                column: "BonLivraisonID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FactureBonLivraisons_FactureID",
                table: "FactureBonLivraisons",
                column: "FactureID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BonLivraisonDetails");

            migrationBuilder.DropTable(
                name: "FactureBonLivraisons");

            migrationBuilder.DropTable(
                name: "BonsLivraison");

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
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FourniProApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAvoir : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Avoirs",
                columns: table => new
                {
                    AvoirId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NumeroAvoir = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    DateAvoir = table.Column<DateOnly>(type: "date", nullable: false),
                    VenteId = table.Column<int>(type: "integer", nullable: true),
                    CreditId = table.Column<int>(type: "integer", nullable: true),
                    ProduitId = table.Column<int>(type: "integer", nullable: true),
                    ClientId = table.Column<int>(type: "integer", nullable: true),
                    EmployeId = table.Column<int>(type: "integer", nullable: true),
                    Quantite = table.Column<int>(type: "integer", nullable: true),
                    PrixUnitaire = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    MontantAvoir = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Raison = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    ReferenceFile = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AjoutePar = table.Column<int>(type: "integer", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiePar = table.Column<int>(type: "integer", nullable: true),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Avoirs", x => x.AvoirId);
                    table.ForeignKey(
                        name: "FK_Avoirs_Credits_CreditId",
                        column: x => x.CreditId,
                        principalTable: "Credits",
                        principalColumn: "CreditId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Avoirs_Employes_EmployeId",
                        column: x => x.EmployeId,
                        principalTable: "Employes",
                        principalColumn: "EmployeId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Avoirs_Ventes_VenteId",
                        column: x => x.VenteId,
                        principalTable: "Ventes",
                        principalColumn: "VenteId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Avoirs_CreditId",
                table: "Avoirs",
                column: "CreditId");

            migrationBuilder.CreateIndex(
                name: "IX_Avoirs_EmployeId",
                table: "Avoirs",
                column: "EmployeId");

            migrationBuilder.CreateIndex(
                name: "IX_Avoirs_VenteId",
                table: "Avoirs",
                column: "VenteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Avoirs");
        }
    }
}

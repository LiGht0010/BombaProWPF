using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FourniProApi.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditFournisseur : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ModePaiement",
                table: "Achats",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CreditsFournisseur",
                columns: table => new
                {
                    CreditFournisseurId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NumeroCreditF = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    DateCredit = table.Column<DateOnly>(type: "date", nullable: false),
                    AchatId = table.Column<int>(type: "integer", nullable: true),
                    FournisseurId = table.Column<int>(type: "integer", nullable: true),
                    EmployeId = table.Column<int>(type: "integer", nullable: true),
                    MontantTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Statut = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ChequeReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StatutCheque = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    AjoutePar = table.Column<int>(type: "integer", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiePar = table.Column<int>(type: "integer", nullable: true),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditsFournisseur", x => x.CreditFournisseurId);
                    table.ForeignKey(
                        name: "FK_CreditsFournisseur_Achats_AchatId",
                        column: x => x.AchatId,
                        principalTable: "Achats",
                        principalColumn: "AchatId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CreditsFournisseur_Employes_EmployeId",
                        column: x => x.EmployeId,
                        principalTable: "Employes",
                        principalColumn: "EmployeId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CreditsFournisseur_Fournisseurs_FournisseurId",
                        column: x => x.FournisseurId,
                        principalTable: "Fournisseurs",
                        principalColumn: "FournisseurId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PaiementsFournisseur",
                columns: table => new
                {
                    PaiementFournisseurId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreditFournisseurId = table.Column<int>(type: "integer", nullable: false),
                    DatePaiement = table.Column<DateOnly>(type: "date", nullable: false),
                    Montant = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    PaymentMethod = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ReferenceFile = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    EmployeId = table.Column<int>(type: "integer", nullable: true),
                    AjoutePar = table.Column<int>(type: "integer", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiePar = table.Column<int>(type: "integer", nullable: true),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaiementsFournisseur", x => x.PaiementFournisseurId);
                    table.ForeignKey(
                        name: "FK_PaiementsFournisseur_CreditsFournisseur_CreditFournisseurId",
                        column: x => x.CreditFournisseurId,
                        principalTable: "CreditsFournisseur",
                        principalColumn: "CreditFournisseurId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaiementsFournisseur_Employes_EmployeId",
                        column: x => x.EmployeId,
                        principalTable: "Employes",
                        principalColumn: "EmployeId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CreditsFournisseur_AchatId",
                table: "CreditsFournisseur",
                column: "AchatId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditsFournisseur_EmployeId",
                table: "CreditsFournisseur",
                column: "EmployeId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditsFournisseur_FournisseurId",
                table: "CreditsFournisseur",
                column: "FournisseurId");

            migrationBuilder.CreateIndex(
                name: "IX_PaiementsFournisseur_CreditFournisseurId",
                table: "PaiementsFournisseur",
                column: "CreditFournisseurId");

            migrationBuilder.CreateIndex(
                name: "IX_PaiementsFournisseur_EmployeId",
                table: "PaiementsFournisseur",
                column: "EmployeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaiementsFournisseur");

            migrationBuilder.DropTable(
                name: "CreditsFournisseur");

            migrationBuilder.DropColumn(
                name: "ModePaiement",
                table: "Achats");
        }
    }
}

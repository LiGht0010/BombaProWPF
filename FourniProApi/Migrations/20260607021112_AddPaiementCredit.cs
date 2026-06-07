using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FourniProApi.Migrations
{
    /// <inheritdoc />
    public partial class AddPaiementCredit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaiementsCredit",
                columns: table => new
                {
                    PaiementCreditId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreditId = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_PaiementsCredit", x => x.PaiementCreditId);
                    table.ForeignKey(
                        name: "FK_PaiementsCredit_Credits_CreditId",
                        column: x => x.CreditId,
                        principalTable: "Credits",
                        principalColumn: "CreditId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaiementsCredit_Employes_EmployeId",
                        column: x => x.EmployeId,
                        principalTable: "Employes",
                        principalColumn: "EmployeId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaiementsCredit_CreditId",
                table: "PaiementsCredit",
                column: "CreditId");

            migrationBuilder.CreateIndex(
                name: "IX_PaiementsCredit_EmployeId",
                table: "PaiementsCredit",
                column: "EmployeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaiementsCredit");
        }
    }
}

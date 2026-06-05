using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FourniProApi.Migrations
{
    /// <inheritdoc />
    public partial class AddCredit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Credits",
                columns: table => new
                {
                    CreditId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NumeroCredit = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    DateCredit = table.Column<DateOnly>(type: "date", nullable: false),
                    ProduitID = table.Column<int>(type: "integer", nullable: true),
                    ClientID = table.Column<int>(type: "integer", nullable: true),
                    VoyageID = table.Column<int>(type: "integer", nullable: true),
                    EmployeId = table.Column<int>(type: "integer", nullable: true),
                    Quantite = table.Column<int>(type: "integer", nullable: true),
                    PrixUnitaire = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Remise = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    MontantTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Statut = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ChequeReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StatutCheque = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    Reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ReferenceFile = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AjoutePar = table.Column<int>(type: "integer", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiePar = table.Column<int>(type: "integer", nullable: true),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Credits", x => x.CreditId);
                    table.ForeignKey(
                        name: "FK_Credits_Employes_EmployeId",
                        column: x => x.EmployeId,
                        principalTable: "Employes",
                        principalColumn: "EmployeId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Credits_EmployeId",
                table: "Credits",
                column: "EmployeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Credits");
        }
    }
}

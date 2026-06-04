using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FourniProApi.Migrations
{
    /// <inheritdoc />
    public partial class AddVente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ventes",
                columns: table => new
                {
                    VenteId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NumeroVente = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    DateVente = table.Column<DateOnly>(type: "date", nullable: false),
                    ProduitID = table.Column<int>(type: "integer", nullable: true),
                    ClientID = table.Column<int>(type: "integer", nullable: true),
                    VoyageID = table.Column<int>(type: "integer", nullable: true),
                    EmployerID = table.Column<int>(type: "integer", nullable: true),
                    Quantite = table.Column<int>(type: "integer", nullable: true),
                    PrixUnitaire = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Remise = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    MontantTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    PaymentMethod = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ReferenceFile = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AjoutePar = table.Column<int>(type: "integer", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiePar = table.Column<int>(type: "integer", nullable: true),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ventes", x => x.VenteId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ventes");
        }
    }
}

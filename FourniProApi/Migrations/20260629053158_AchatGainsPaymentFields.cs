using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FourniProApi.Migrations
{
    /// <inheritdoc />
    public partial class AchatGainsPaymentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChequeReference",
                table: "Achats",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MontantPaye",
                table: "Achats",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Statut",
                table: "Achats",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatutCheque",
                table: "Achats",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChequeReference",
                table: "Achats");

            migrationBuilder.DropColumn(
                name: "MontantPaye",
                table: "Achats");

            migrationBuilder.DropColumn(
                name: "Statut",
                table: "Achats");

            migrationBuilder.DropColumn(
                name: "StatutCheque",
                table: "Achats");
        }
    }
}

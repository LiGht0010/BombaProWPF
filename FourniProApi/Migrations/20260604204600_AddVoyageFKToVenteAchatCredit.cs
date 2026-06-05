using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FourniProApi.Migrations
{
    /// <inheritdoc />
    public partial class AddVoyageFKToVenteAchatCredit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VoyageID",
                table: "Achats",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ventes_VoyageID",
                table: "Ventes",
                column: "VoyageID");

            migrationBuilder.CreateIndex(
                name: "IX_Credits_VoyageID",
                table: "Credits",
                column: "VoyageID");

            migrationBuilder.CreateIndex(
                name: "IX_Achats_VoyageID",
                table: "Achats",
                column: "VoyageID");

            migrationBuilder.AddForeignKey(
                name: "FK_Achats_Voyages_VoyageID",
                table: "Achats",
                column: "VoyageID",
                principalTable: "Voyages",
                principalColumn: "VoyageId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Credits_Voyages_VoyageID",
                table: "Credits",
                column: "VoyageID",
                principalTable: "Voyages",
                principalColumn: "VoyageId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Ventes_Voyages_VoyageID",
                table: "Ventes",
                column: "VoyageID",
                principalTable: "Voyages",
                principalColumn: "VoyageId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Achats_Voyages_VoyageID",
                table: "Achats");

            migrationBuilder.DropForeignKey(
                name: "FK_Credits_Voyages_VoyageID",
                table: "Credits");

            migrationBuilder.DropForeignKey(
                name: "FK_Ventes_Voyages_VoyageID",
                table: "Ventes");

            migrationBuilder.DropIndex(
                name: "IX_Ventes_VoyageID",
                table: "Ventes");

            migrationBuilder.DropIndex(
                name: "IX_Credits_VoyageID",
                table: "Credits");

            migrationBuilder.DropIndex(
                name: "IX_Achats_VoyageID",
                table: "Achats");

            migrationBuilder.DropColumn(
                name: "VoyageID",
                table: "Achats");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FourniProApi.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeIdToAchat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmployeId",
                table: "Achats",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Achats_EmployeId",
                table: "Achats",
                column: "EmployeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Achats_Employes_EmployeId",
                table: "Achats",
                column: "EmployeId",
                principalTable: "Employes",
                principalColumn: "EmployeId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Achats_Employes_EmployeId",
                table: "Achats");

            migrationBuilder.DropIndex(
                name: "IX_Achats_EmployeId",
                table: "Achats");

            migrationBuilder.DropColumn(
                name: "EmployeId",
                table: "Achats");
        }
    }
}

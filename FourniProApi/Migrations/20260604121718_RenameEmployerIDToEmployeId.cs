using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FourniProApi.Migrations
{
    /// <inheritdoc />
    public partial class RenameEmployerIDToEmployeId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ventes_Employes_EmployerID",
                table: "Ventes");

            migrationBuilder.RenameColumn(
                name: "EmployerID",
                table: "Ventes",
                newName: "EmployeId");

            migrationBuilder.RenameIndex(
                name: "IX_Ventes_EmployerID",
                table: "Ventes",
                newName: "IX_Ventes_EmployeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ventes_Employes_EmployeId",
                table: "Ventes",
                column: "EmployeId",
                principalTable: "Employes",
                principalColumn: "EmployeId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ventes_Employes_EmployeId",
                table: "Ventes");

            migrationBuilder.RenameColumn(
                name: "EmployeId",
                table: "Ventes",
                newName: "EmployerID");

            migrationBuilder.RenameIndex(
                name: "IX_Ventes_EmployeId",
                table: "Ventes",
                newName: "IX_Ventes_EmployerID");

            migrationBuilder.AddForeignKey(
                name: "FK_Ventes_Employes_EmployerID",
                table: "Ventes",
                column: "EmployerID",
                principalTable: "Employes",
                principalColumn: "EmployeId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}

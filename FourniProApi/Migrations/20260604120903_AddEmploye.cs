using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FourniProApi.Migrations
{
    /// <inheritdoc />
    public partial class AddEmploye : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Employes",
                columns: table => new
                {
                    EmployeId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nom = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Prenom = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CIN = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Telephone = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    Address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Poste = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Salaire = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    AjoutePar = table.Column<int>(type: "integer", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiePar = table.Column<int>(type: "integer", nullable: true),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employes", x => x.EmployeId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ventes_EmployerID",
                table: "Ventes",
                column: "EmployerID");

            migrationBuilder.AddForeignKey(
                name: "FK_Ventes_Employes_EmployerID",
                table: "Ventes",
                column: "EmployerID",
                principalTable: "Employes",
                principalColumn: "EmployeId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ventes_Employes_EmployerID",
                table: "Ventes");

            migrationBuilder.DropTable(
                name: "Employes");

            migrationBuilder.DropIndex(
                name: "IX_Ventes_EmployerID",
                table: "Ventes");
        }
    }
}

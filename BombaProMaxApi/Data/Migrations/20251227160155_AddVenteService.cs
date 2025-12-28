using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BombaProMaxApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVenteService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VenteServices",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NumeroVente = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DateVente = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ServiceID = table.Column<int>(type: "integer", nullable: false),
                    Quantite = table.Column<int>(type: "integer", nullable: false),
                    PrixUnitaire = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    ClientID = table.Column<int>(type: "integer", nullable: true),
                    EmployeID = table.Column<int>(type: "integer", nullable: true),
                    MoyenPaiementID = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Statut = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreePar = table.Column<int>(type: "integer", nullable: true),
                    ModifiePar = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VenteServices", x => x.ID);
                    table.ForeignKey(
                        name: "FK_VenteServices_Clients_ClientID",
                        column: x => x.ClientID,
                        principalTable: "Clients",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_VenteServices_Employes_EmployeID",
                        column: x => x.EmployeID,
                        principalTable: "Employes",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_VenteServices_MoyensPaiement_MoyenPaiementID",
                        column: x => x.MoyenPaiementID,
                        principalTable: "MoyensPaiement",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_VenteServices_Services_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "Services",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 27, 16, 1, 54, 647, DateTimeKind.Utc).AddTicks(5890));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 27, 16, 1, 54, 647, DateTimeKind.Utc).AddTicks(5904));

            migrationBuilder.CreateIndex(
                name: "IX_VenteServices_ClientID",
                table: "VenteServices",
                column: "ClientID");

            migrationBuilder.CreateIndex(
                name: "IX_VenteServices_EmployeID",
                table: "VenteServices",
                column: "EmployeID");

            migrationBuilder.CreateIndex(
                name: "IX_VenteServices_MoyenPaiementID",
                table: "VenteServices",
                column: "MoyenPaiementID");

            migrationBuilder.CreateIndex(
                name: "IX_VenteServices_ServiceID",
                table: "VenteServices",
                column: "ServiceID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VenteServices");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 26, 23, 47, 31, 851, DateTimeKind.Utc).AddTicks(6005));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 26, 23, 47, 31, 851, DateTimeKind.Utc).AddTicks(6018));
        }
    }
}

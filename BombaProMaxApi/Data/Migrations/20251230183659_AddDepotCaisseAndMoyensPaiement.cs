using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BombaProMaxApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDepotCaisseAndMoyensPaiement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DepotsCaisse",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Montant = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DateDepot = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReferenceBancaire = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Banque = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ValidePar = table.Column<int>(type: "integer", nullable: true),
                    AjoutePar = table.Column<int>(type: "integer", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiePar = table.Column<int>(type: "integer", nullable: true),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepotsCaisse", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DepotsCaisse_Users_ValidePar",
                        column: x => x.ValidePar,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.InsertData(
                table: "MoyensPaiement",
                columns: new[] { "ID", "Nom" },
                values: new object[,]
                {
                    { 1, "Espèces" },
                    { 2, "TPE" },
                    { 3, "Chèque" }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 30, 18, 36, 58, 407, DateTimeKind.Utc).AddTicks(841));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 30, 18, 36, 58, 407, DateTimeKind.Utc).AddTicks(856));

            migrationBuilder.CreateIndex(
                name: "IX_DepotsCaisse_ValidePar",
                table: "DepotsCaisse",
                column: "ValidePar");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DepotsCaisse");

            migrationBuilder.DeleteData(
                table: "MoyensPaiement",
                keyColumn: "ID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "MoyensPaiement",
                keyColumn: "ID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "MoyensPaiement",
                keyColumn: "ID",
                keyValue: 3);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 28, 15, 24, 56, 729, DateTimeKind.Utc).AddTicks(7006));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 28, 15, 24, 56, 729, DateTimeKind.Utc).AddTicks(7193));
        }
    }
}

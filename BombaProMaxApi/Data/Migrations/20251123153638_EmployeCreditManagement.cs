using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BombaProMaxApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class EmployeCreditManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeBilanCredits",
                columns: table => new
                {
                    BilanID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeID = table.Column<int>(type: "integer", nullable: false),
                    TotalCredit = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    TotalPaye = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric(12,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeBilanCredits", x => x.BilanID);
                    table.ForeignKey(
                        name: "FK_EmployeBilanCredits_Employes_EmployeID",
                        column: x => x.EmployeID,
                        principalTable: "Employes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeCreditTransactions",
                columns: table => new
                {
                    CreditID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NumeroTransaction = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    EmployeID = table.Column<int>(type: "integer", nullable: false),
                    MontantTotal = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    DateCredit = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AjoutePar = table.Column<int>(type: "integer", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiePar = table.Column<int>(type: "integer", nullable: true),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeCreditTransactions", x => x.CreditID);
                    table.ForeignKey(
                        name: "FK_EmployeCreditTransactions_Employes_EmployeID",
                        column: x => x.EmployeID,
                        principalTable: "Employes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeReglementCredits",
                columns: table => new
                {
                    ReglementID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeID = table.Column<int>(type: "integer", nullable: false),
                    MontantPaye = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    ModePaiementID = table.Column<int>(type: "integer", nullable: false),
                    ReferenceTransaction = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ValidePar = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DateReglement = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Remarques = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AjoutePar = table.Column<int>(type: "integer", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiePar = table.Column<int>(type: "integer", nullable: true),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeReglementCredits", x => x.ReglementID);
                    table.ForeignKey(
                        name: "FK_EmployeReglementCredits_Employes_EmployeID",
                        column: x => x.EmployeID,
                        principalTable: "Employes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeReglementCredits_MoyensPaiement_ModePaiementID",
                        column: x => x.ModePaiementID,
                        principalTable: "MoyensPaiement",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2025, 11, 23, 15, 36, 36, 720, DateTimeKind.Utc).AddTicks(8222));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2025, 11, 23, 15, 36, 36, 720, DateTimeKind.Utc).AddTicks(8234));

            migrationBuilder.CreateIndex(
                name: "IX_EmployeBilanCredits_EmployeID",
                table: "EmployeBilanCredits",
                column: "EmployeID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeCreditTransactions_EmployeID",
                table: "EmployeCreditTransactions",
                column: "EmployeID");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeReglementCredits_EmployeID",
                table: "EmployeReglementCredits",
                column: "EmployeID");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeReglementCredits_ModePaiementID",
                table: "EmployeReglementCredits",
                column: "ModePaiementID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeBilanCredits");

            migrationBuilder.DropTable(
                name: "EmployeCreditTransactions");

            migrationBuilder.DropTable(
                name: "EmployeReglementCredits");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2025, 11, 23, 14, 50, 20, 848, DateTimeKind.Utc).AddTicks(534));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2025, 11, 23, 14, 50, 20, 848, DateTimeKind.Utc).AddTicks(545));
        }
    }
}

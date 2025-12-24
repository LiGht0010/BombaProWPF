using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BombaProMaxApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReservoirCalibrationAndManufacturerInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DiametreMm",
                table: "Reservoirs",
                type: "numeric(8,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Fabricant",
                table: "Reservoirs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HauteurMax",
                table: "Reservoirs",
                type: "numeric(6,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroSerie",
                table: "Reservoirs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ReservoirCalibrations",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReservoirID = table.Column<int>(type: "integer", nullable: false),
                    HauteurCm = table.Column<decimal>(type: "numeric(6,2)", nullable: false),
                    VolumeLitres = table.Column<decimal>(type: "numeric(12,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservoirCalibrations", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ReservoirCalibrations_Reservoirs_ReservoirID",
                        column: x => x.ReservoirID,
                        principalTable: "Reservoirs",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 20, 11, 7, 16, 65, DateTimeKind.Utc).AddTicks(5871));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 20, 11, 7, 16, 65, DateTimeKind.Utc).AddTicks(5882));

            migrationBuilder.CreateIndex(
                name: "IX_ReservoirCalibrations_ReservoirID_HauteurCm",
                table: "ReservoirCalibrations",
                columns: new[] { "ReservoirID", "HauteurCm" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReservoirCalibrations");

            migrationBuilder.DropColumn(
                name: "DiametreMm",
                table: "Reservoirs");

            migrationBuilder.DropColumn(
                name: "Fabricant",
                table: "Reservoirs");

            migrationBuilder.DropColumn(
                name: "HauteurMax",
                table: "Reservoirs");

            migrationBuilder.DropColumn(
                name: "NumeroSerie",
                table: "Reservoirs");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 13, 19, 58, 35, 930, DateTimeKind.Utc).AddTicks(8203));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 13, 19, 58, 35, 930, DateTimeKind.Utc).AddTicks(8216));
        }
    }
}

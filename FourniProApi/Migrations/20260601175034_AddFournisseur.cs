using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FourniProApi.Migrations
{
    /// <inheritdoc />
    public partial class AddFournisseur : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Fournisseurs",
                columns: table => new
                {
                    FournisseurId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Prenom = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Nom = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Societe = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Adresse = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Telephone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RIB = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Contact = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ConditionsPaiement = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Statut = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AjoutePar = table.Column<int>(type: "integer", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiePar = table.Column<int>(type: "integer", nullable: true),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fournisseurs", x => x.FournisseurId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Fournisseurs");
        }
    }
}

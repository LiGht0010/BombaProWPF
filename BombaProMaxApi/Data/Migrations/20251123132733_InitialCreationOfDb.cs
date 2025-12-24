using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BombaProMaxApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreationOfDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NumeroClient = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Nom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Contact = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CIN = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NomSociete = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    userID = table.Column<int>(type: "integer", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Depenses",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Numero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: true),
                    Categorie = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Montant = table.Column<decimal>(type: "numeric(10,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Depenses", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Employes",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nom = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Prenom = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CIN = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Telephone = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    Address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Poste = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Salaire = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    AjoutePar = table.Column<int>(type: "integer", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiePar = table.Column<int>(type: "integer", nullable: true),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employes", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Fournisseurs",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
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
                    Statut = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fournisseurs", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "IndicateursFinanciers",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DebutPeriode = table.Column<DateOnly>(type: "date", nullable: true),
                    FinPeriode = table.Column<DateOnly>(type: "date", nullable: true),
                    ChiffreAffairesTotal = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    DepensesTotales = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    BeneficeNet = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    TotalAchats = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    TotalVentes = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    TotalCarburantVendu = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    TotalProduitsVendus = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    TotalServicesVendus = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndicateursFinanciers", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "JoursActivité",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    ChiffreAffaires = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    FraisExploitation = table.Column<decimal>(type: "numeric(12,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JoursActivité", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "MoyensPaiement",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nom = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MoyensPaiement", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Numero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Prix = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    AjoutePar = table.Column<int>(type: "integer", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiePar = table.Column<int>(type: "integer", nullable: true),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    IsAdmin = table.Column<bool>(type: "boolean", nullable: false),
                    IsSuperAdmin = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    createdBy = table.Column<int>(type: "integer", nullable: false),
                    updatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CanManageUsers = table.Column<bool>(type: "boolean", nullable: false),
                    CanManageProducts = table.Column<bool>(type: "boolean", nullable: false),
                    CanViewReports = table.Column<bool>(type: "boolean", nullable: false),
                    CanManageSettings = table.Column<bool>(type: "boolean", nullable: false),
                    CanManageSales = table.Column<bool>(type: "boolean", nullable: false),
                    CanManagePromotions = table.Column<bool>(type: "boolean", nullable: false),
                    CanManageCustomers = table.Column<bool>(type: "boolean", nullable: false),
                    CanManageSuppliers = table.Column<bool>(type: "boolean", nullable: false),
                    CanManageCategories = table.Column<bool>(type: "boolean", nullable: false),
                    ShowAcceuil = table.Column<bool>(type: "boolean", nullable: false),
                    ShowTableauDeBord = table.Column<bool>(type: "boolean", nullable: false),
                    ShowVente = table.Column<bool>(type: "boolean", nullable: false),
                    EditLivreur = table.Column<bool>(type: "boolean", nullable: false),
                    AddAchat = table.Column<bool>(type: "boolean", nullable: false),
                    EditCiternes = table.Column<bool>(type: "boolean", nullable: false),
                    EditPistolets = table.Column<bool>(type: "boolean", nullable: false),
                    EditClients = table.Column<bool>(type: "boolean", nullable: false),
                    AddBonFacturation = table.Column<bool>(type: "boolean", nullable: false),
                    ShowDepenses = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Produits",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NumeroProduit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    PrixAchat = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    PrixHT = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    TVA = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    PrixTTC = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    Stock = table.Column<int>(type: "integer", nullable: true),
                    StockMinimum = table.Column<int>(type: "integer", nullable: true),
                    DelaiDeLivraison = table.Column<int>(type: "integer", nullable: true),
                    CategorieID = table.Column<int>(type: "integer", nullable: true),
                    AjoutePar = table.Column<int>(type: "integer", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiePar = table.Column<int>(type: "integer", nullable: true),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produits", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Produits_Categories_CategorieID",
                        column: x => x.CategorieID,
                        principalTable: "Categories",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "BilansCredit",
                columns: table => new
                {
                    BilanID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClientID = table.Column<int>(type: "integer", nullable: false),
                    TotalCredit = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    TotalPaye = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    CreditFacture = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    CreditNonFacture = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    DerniereMiseAJour = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodeDebut = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PeriodeFin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BilansCredit", x => x.BilanID);
                    table.ForeignKey(
                        name: "FK_BilansCredit_Clients_ClientID",
                        column: x => x.ClientID,
                        principalTable: "Clients",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Jaugeages",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DateJaugeage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TemoinID = table.Column<int>(type: "integer", nullable: false),
                    NumeroJaugeage = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Observations = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AjoutePar = table.Column<int>(type: "integer", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiePar = table.Column<int>(type: "integer", nullable: true),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jaugeages", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Jaugeages_Employes_TemoinID",
                        column: x => x.TemoinID,
                        principalTable: "Employes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Periodes",
                columns: table => new
                {
                    PeriodeID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DateDebut = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateFin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EmployeID = table.Column<int>(type: "integer", nullable: true),
                    AjoutePar = table.Column<int>(type: "integer", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiePar = table.Column<int>(type: "integer", nullable: true),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Periodes", x => x.PeriodeID);
                    table.ForeignKey(
                        name: "FK_Periodes_Employes_EmployeID",
                        column: x => x.EmployeID,
                        principalTable: "Employes",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Chauffeurs",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nom = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Prenom = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CIN = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Telephone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    NumeroPermis = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FournisseurID = table.Column<int>(type: "integer", nullable: true),
                    AjoutePar = table.Column<int>(type: "integer", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiePar = table.Column<int>(type: "integer", nullable: true),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chauffeurs", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Chauffeurs_Fournisseurs_FournisseurID",
                        column: x => x.FournisseurID,
                        principalTable: "Fournisseurs",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Citernes",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MatriculeCiterne = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Capacite = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    PartitionsNumber = table.Column<long>(type: "bigint", nullable: true),
                    FournisseurID = table.Column<int>(type: "integer", nullable: false),
                    AjoutePar = table.Column<int>(type: "integer", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiePar = table.Column<int>(type: "integer", nullable: true),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Citernes", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Citernes_Fournisseurs_FournisseurID",
                        column: x => x.FournisseurID,
                        principalTable: "Fournisseurs",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Factures",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NumeroFacture = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DateFacture = table.Column<DateOnly>(type: "date", nullable: true),
                    ClientID = table.Column<int>(type: "integer", nullable: true),
                    MontantTotal = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    Statut = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MoyenPaiementID = table.Column<int>(type: "integer", nullable: true),
                    DatePaiement = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Factures", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Factures_Clients_ClientID",
                        column: x => x.ClientID,
                        principalTable: "Clients",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Factures_MoyensPaiement_MoyenPaiementID",
                        column: x => x.MoyenPaiementID,
                        principalTable: "MoyensPaiement",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "ReglementsCredit",
                columns: table => new
                {
                    ReglementID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClientID = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_ReglementsCredit", x => x.ReglementID);
                    table.ForeignKey(
                        name: "FK_ReglementsCredit_Clients_ClientID",
                        column: x => x.ClientID,
                        principalTable: "Clients",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReglementsCredit_MoyensPaiement_ModePaiementID",
                        column: x => x.ModePaiementID,
                        principalTable: "MoyensPaiement",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reservoirs",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Numero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ProduitID = table.Column<int>(type: "integer", nullable: true),
                    Capacite = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    NiveauDeCarburant = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    AjoutePar = table.Column<int>(type: "integer", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiePar = table.Column<int>(type: "integer", nullable: true),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservoirs", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Reservoirs_Produits_ProduitID",
                        column: x => x.ProduitID,
                        principalTable: "Produits",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "VenteLubrifiantsEtArticles",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NumeroVente = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DateVente = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProduitID = table.Column<int>(type: "integer", nullable: false),
                    QuantiteVendue = table.Column<int>(type: "integer", nullable: false),
                    PrixUnitaireTTC = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    ClientID = table.Column<int>(type: "integer", nullable: true),
                    EmployeID = table.Column<int>(type: "integer", nullable: true),
                    MoyenPaiementID = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Statut = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreePar = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ModifiePar = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VenteLubrifiantsEtArticles", x => x.ID);
                    table.ForeignKey(
                        name: "FK_VenteLubrifiantsEtArticles_Clients_ClientID",
                        column: x => x.ClientID,
                        principalTable: "Clients",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_VenteLubrifiantsEtArticles_Employes_EmployeID",
                        column: x => x.EmployeID,
                        principalTable: "Employes",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_VenteLubrifiantsEtArticles_MoyensPaiement_MoyenPaiementID",
                        column: x => x.MoyenPaiementID,
                        principalTable: "MoyensPaiement",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_VenteLubrifiantsEtArticles_Produits_ProduitID",
                        column: x => x.ProduitID,
                        principalTable: "Produits",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Camions",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Matricule = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Marque = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CiterneID = table.Column<int>(type: "integer", nullable: true),
                    FournisseurID = table.Column<int>(type: "integer", nullable: false),
                    AjoutePar = table.Column<int>(type: "integer", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiePar = table.Column<int>(type: "integer", nullable: true),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Camions", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Camions_Citernes_CiterneID",
                        column: x => x.CiterneID,
                        principalTable: "Citernes",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Camions_Fournisseurs_FournisseurID",
                        column: x => x.FournisseurID,
                        principalTable: "Fournisseurs",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CreditTransactions",
                columns: table => new
                {
                    CreditID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NumeroTransaction = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ClientID = table.Column<int>(type: "integer", nullable: false),
                    ProduitID = table.Column<int>(type: "integer", nullable: true),
                    ServiceID = table.Column<int>(type: "integer", nullable: true),
                    PrixTTC = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Quantite = table.Column<int>(type: "integer", nullable: false),
                    MontantTotal = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    DateCredit = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Facture = table.Column<bool>(type: "boolean", nullable: false),
                    FactureID = table.Column<int>(type: "integer", nullable: true),
                    AjoutePar = table.Column<int>(type: "integer", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiePar = table.Column<int>(type: "integer", nullable: true),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditTransactions", x => x.CreditID);
                    table.ForeignKey(
                        name: "FK_CreditTransactions_Clients_ClientID",
                        column: x => x.ClientID,
                        principalTable: "Clients",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CreditTransactions_Factures_FactureID",
                        column: x => x.FactureID,
                        principalTable: "Factures",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_CreditTransactions_Produits_ProduitID",
                        column: x => x.ProduitID,
                        principalTable: "Produits",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_CreditTransactions_Services_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "Services",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "ÉlémentsFacture",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FactureID = table.Column<int>(type: "integer", nullable: true),
                    ProduitID = table.Column<int>(type: "integer", nullable: true),
                    ServiceID = table.Column<int>(type: "integer", nullable: true),
                    Quantite = table.Column<int>(type: "integer", nullable: true),
                    PrixUnitaire = table.Column<decimal>(type: "numeric(10,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ÉlémentsFacture", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ÉlémentsFacture_Factures_FactureID",
                        column: x => x.FactureID,
                        principalTable: "Factures",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_ÉlémentsFacture_Produits_ProduitID",
                        column: x => x.ProduitID,
                        principalTable: "Produits",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_ÉlémentsFacture_Services_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "Services",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "JaugeageDetails",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JaugeageID = table.Column<int>(type: "integer", nullable: false),
                    ReservoirID = table.Column<int>(type: "integer", nullable: false),
                    HauteurMesuree = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    VolumeCalcule = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Temperature = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JaugeageDetails", x => x.ID);
                    table.ForeignKey(
                        name: "FK_JaugeageDetails_Jaugeages_JaugeageID",
                        column: x => x.JaugeageID,
                        principalTable: "Jaugeages",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JaugeageDetails_Reservoirs_ReservoirID",
                        column: x => x.ReservoirID,
                        principalTable: "Reservoirs",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pompes",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Numero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Statut = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CompteurElectroniqueActuel = table.Column<decimal>(type: "numeric(18,3)", nullable: true),
                    CompteurMecaniqueActuel = table.Column<decimal>(type: "numeric(18,3)", nullable: true),
                    ReservoirAssocieID = table.Column<int>(type: "integer", nullable: false),
                    AjoutePar = table.Column<int>(type: "integer", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiePar = table.Column<int>(type: "integer", nullable: true),
                    DateModification = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pompes", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Pompes_Reservoirs_ReservoirAssocieID",
                        column: x => x.ReservoirAssocieID,
                        principalTable: "Reservoirs",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Achats",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Numero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    FournisseurID = table.Column<int>(type: "integer", nullable: true),
                    ProduitID = table.Column<int>(type: "integer", nullable: true),
                    Quantite = table.Column<int>(type: "integer", nullable: true),
                    Cout = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    PrixAchatUnitaire = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    ChauffeurID = table.Column<int>(type: "integer", nullable: true),
                    CamionID = table.Column<int>(type: "integer", nullable: true),
                    LivraisonDefectueuse = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Achats", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Achats_Camions_CamionID",
                        column: x => x.CamionID,
                        principalTable: "Camions",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Achats_Chauffeurs_ChauffeurID",
                        column: x => x.ChauffeurID,
                        principalTable: "Chauffeurs",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Achats_Fournisseurs_FournisseurID",
                        column: x => x.FournisseurID,
                        principalTable: "Fournisseurs",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Achats_Produits_ProduitID",
                        column: x => x.ProduitID,
                        principalTable: "Produits",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "PeriodeDetails",
                columns: table => new
                {
                    PeriodeDetailID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PeriodeID = table.Column<int>(type: "integer", nullable: false),
                    PompeID = table.Column<int>(type: "integer", nullable: false),
                    ReservoirID = table.Column<int>(type: "integer", nullable: false),
                    ProduitID = table.Column<int>(type: "integer", nullable: false),
                    PrixCarburant = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    CompteurElectroniqueDebut = table.Column<decimal>(type: "numeric", nullable: false),
                    CompteurElectroniqueFinal = table.Column<decimal>(type: "numeric", nullable: false),
                    CompteurMecaniqueDebut = table.Column<decimal>(type: "numeric", nullable: false),
                    CompteurMecaniqueFinal = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodeDetails", x => x.PeriodeDetailID);
                    table.ForeignKey(
                        name: "FK_PeriodeDetails_Periodes_PeriodeID",
                        column: x => x.PeriodeID,
                        principalTable: "Periodes",
                        principalColumn: "PeriodeID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PeriodeDetails_Pompes_PompeID",
                        column: x => x.PompeID,
                        principalTable: "Pompes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PeriodeDetails_Produits_ProduitID",
                        column: x => x.ProduitID,
                        principalTable: "Produits",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PeriodeDetails_Reservoirs_ReservoirID",
                        column: x => x.ReservoirID,
                        principalTable: "Reservoirs",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AchatAllocations",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AchatID = table.Column<int>(type: "integer", nullable: false),
                    ReservoirID = table.Column<int>(type: "integer", nullable: false),
                    QuantiteAllouee = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    DateAllocation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Statut = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UtilisateurAllocation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchatAllocations", x => x.ID);
                    table.ForeignKey(
                        name: "FK_AchatAllocations_Achats_AchatID",
                        column: x => x.AchatID,
                        principalTable: "Achats",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AchatAllocations_Reservoirs_ReservoirID",
                        column: x => x.ReservoirID,
                        principalTable: "Reservoirs",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "AddAchat", "AddBonFacturation", "CanManageCategories", "CanManageCustomers", "CanManageProducts", "CanManagePromotions", "CanManageSales", "CanManageSettings", "CanManageSuppliers", "CanManageUsers", "CanViewReports", "CreatedAt", "EditCiternes", "EditClients", "EditLivreur", "EditPistolets", "Email", "IsActive", "IsAdmin", "IsSuperAdmin", "Name", "Password", "ShowAcceuil", "ShowDepenses", "ShowTableauDeBord", "ShowVente", "UpdatedAt", "createdBy", "updatedBy" },
                values: new object[,]
                {
                    { 5, true, true, true, true, true, true, true, true, true, true, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, true, true, true, "admin@bombapromax.com", true, true, false, "Admin User", "Admin@123", true, true, true, true, new DateTime(2025, 11, 23, 13, 27, 32, 618, DateTimeKind.Utc).AddTicks(1063), 0, 0 },
                    { 6, true, true, true, true, true, true, true, true, true, true, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, true, true, true, "superadmin@bombapromax.com", true, true, true, "Super Admin", "SuperAdmin@123", true, true, true, true, new DateTime(2025, 11, 23, 13, 27, 32, 618, DateTimeKind.Utc).AddTicks(1075), 0, 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AchatAllocations_AchatID",
                table: "AchatAllocations",
                column: "AchatID");

            migrationBuilder.CreateIndex(
                name: "IX_AchatAllocations_ReservoirID",
                table: "AchatAllocations",
                column: "ReservoirID");

            migrationBuilder.CreateIndex(
                name: "IX_Achats_CamionID",
                table: "Achats",
                column: "CamionID");

            migrationBuilder.CreateIndex(
                name: "IX_Achats_ChauffeurID",
                table: "Achats",
                column: "ChauffeurID");

            migrationBuilder.CreateIndex(
                name: "IX_Achats_FournisseurID",
                table: "Achats",
                column: "FournisseurID");

            migrationBuilder.CreateIndex(
                name: "IX_Achats_ProduitID",
                table: "Achats",
                column: "ProduitID");

            migrationBuilder.CreateIndex(
                name: "IX_BilansCredit_ClientID",
                table: "BilansCredit",
                column: "ClientID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Camions_CiterneID",
                table: "Camions",
                column: "CiterneID");

            migrationBuilder.CreateIndex(
                name: "IX_Camions_FournisseurID",
                table: "Camions",
                column: "FournisseurID");

            migrationBuilder.CreateIndex(
                name: "IX_Chauffeurs_FournisseurID",
                table: "Chauffeurs",
                column: "FournisseurID");

            migrationBuilder.CreateIndex(
                name: "IX_Citernes_FournisseurID",
                table: "Citernes",
                column: "FournisseurID");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_CIN",
                table: "Clients",
                column: "CIN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_NumeroClient",
                table: "Clients",
                column: "NumeroClient",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CreditTransactions_ClientID",
                table: "CreditTransactions",
                column: "ClientID");

            migrationBuilder.CreateIndex(
                name: "IX_CreditTransactions_FactureID",
                table: "CreditTransactions",
                column: "FactureID");

            migrationBuilder.CreateIndex(
                name: "IX_CreditTransactions_ProduitID",
                table: "CreditTransactions",
                column: "ProduitID");

            migrationBuilder.CreateIndex(
                name: "IX_CreditTransactions_ServiceID",
                table: "CreditTransactions",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_ÉlémentsFacture_FactureID",
                table: "ÉlémentsFacture",
                column: "FactureID");

            migrationBuilder.CreateIndex(
                name: "IX_ÉlémentsFacture_ProduitID",
                table: "ÉlémentsFacture",
                column: "ProduitID");

            migrationBuilder.CreateIndex(
                name: "IX_ÉlémentsFacture_ServiceID",
                table: "ÉlémentsFacture",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_Factures_ClientID",
                table: "Factures",
                column: "ClientID");

            migrationBuilder.CreateIndex(
                name: "IX_Factures_MoyenPaiementID",
                table: "Factures",
                column: "MoyenPaiementID");

            migrationBuilder.CreateIndex(
                name: "IX_JaugeageDetails_JaugeageID",
                table: "JaugeageDetails",
                column: "JaugeageID");

            migrationBuilder.CreateIndex(
                name: "IX_JaugeageDetails_ReservoirID",
                table: "JaugeageDetails",
                column: "ReservoirID");

            migrationBuilder.CreateIndex(
                name: "IX_Jaugeages_TemoinID",
                table: "Jaugeages",
                column: "TemoinID");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodeDetails_PeriodeID",
                table: "PeriodeDetails",
                column: "PeriodeID");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodeDetails_PompeID",
                table: "PeriodeDetails",
                column: "PompeID");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodeDetails_ProduitID",
                table: "PeriodeDetails",
                column: "ProduitID");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodeDetails_ReservoirID",
                table: "PeriodeDetails",
                column: "ReservoirID");

            migrationBuilder.CreateIndex(
                name: "IX_Periodes_EmployeID",
                table: "Periodes",
                column: "EmployeID");

            migrationBuilder.CreateIndex(
                name: "IX_Pompes_ReservoirAssocieID",
                table: "Pompes",
                column: "ReservoirAssocieID");

            migrationBuilder.CreateIndex(
                name: "IX_Produits_CategorieID",
                table: "Produits",
                column: "CategorieID");

            migrationBuilder.CreateIndex(
                name: "IX_ReglementsCredit_ClientID",
                table: "ReglementsCredit",
                column: "ClientID");

            migrationBuilder.CreateIndex(
                name: "IX_ReglementsCredit_ModePaiementID",
                table: "ReglementsCredit",
                column: "ModePaiementID");

            migrationBuilder.CreateIndex(
                name: "IX_Reservoirs_ProduitID",
                table: "Reservoirs",
                column: "ProduitID");

            migrationBuilder.CreateIndex(
                name: "IX_VenteLubrifiantsEtArticles_ClientID",
                table: "VenteLubrifiantsEtArticles",
                column: "ClientID");

            migrationBuilder.CreateIndex(
                name: "IX_VenteLubrifiantsEtArticles_EmployeID",
                table: "VenteLubrifiantsEtArticles",
                column: "EmployeID");

            migrationBuilder.CreateIndex(
                name: "IX_VenteLubrifiantsEtArticles_MoyenPaiementID",
                table: "VenteLubrifiantsEtArticles",
                column: "MoyenPaiementID");

            migrationBuilder.CreateIndex(
                name: "IX_VenteLubrifiantsEtArticles_ProduitID",
                table: "VenteLubrifiantsEtArticles",
                column: "ProduitID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AchatAllocations");

            migrationBuilder.DropTable(
                name: "BilansCredit");

            migrationBuilder.DropTable(
                name: "CreditTransactions");

            migrationBuilder.DropTable(
                name: "Depenses");

            migrationBuilder.DropTable(
                name: "ÉlémentsFacture");

            migrationBuilder.DropTable(
                name: "IndicateursFinanciers");

            migrationBuilder.DropTable(
                name: "JaugeageDetails");

            migrationBuilder.DropTable(
                name: "JoursActivité");

            migrationBuilder.DropTable(
                name: "PeriodeDetails");

            migrationBuilder.DropTable(
                name: "ReglementsCredit");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "VenteLubrifiantsEtArticles");

            migrationBuilder.DropTable(
                name: "Achats");

            migrationBuilder.DropTable(
                name: "Factures");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropTable(
                name: "Jaugeages");

            migrationBuilder.DropTable(
                name: "Periodes");

            migrationBuilder.DropTable(
                name: "Pompes");

            migrationBuilder.DropTable(
                name: "Camions");

            migrationBuilder.DropTable(
                name: "Chauffeurs");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "MoyensPaiement");

            migrationBuilder.DropTable(
                name: "Employes");

            migrationBuilder.DropTable(
                name: "Reservoirs");

            migrationBuilder.DropTable(
                name: "Citernes");

            migrationBuilder.DropTable(
                name: "Produits");

            migrationBuilder.DropTable(
                name: "Fournisseurs");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}

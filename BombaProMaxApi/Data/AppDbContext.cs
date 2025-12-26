namespace BombaProMaxApi.Data;
using Microsoft.EntityFrameworkCore;
using BombaProMaxApi.Models;
using Npgsql.EntityFrameworkCore.PostgreSQL;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<User> Users { get; set; } = default!;
    public DbSet<Client> Clients { get; set; } = default!;
    public DbSet<Achat> Achats { get; set; } = default!;
    public DbSet<AchatAllocation> AchatAllocations { get; set; } = default!;
    public DbSet<BilanCredit> BilansCredit { get; set; } = default!;
    public DbSet<Camion> Camions { get; set; } = default!;
    public DbSet<Categorie> Categories { get; set; } = default!;
    public DbSet<Chauffeur> Chauffeurs { get; set; } = default!;
    public DbSet<Citerne> Citernes { get; set; } = default!;
    public DbSet<CreditTransaction> CreditTransactions { get; set; } = default!;
    public DbSet<Depense> Depenses { get; set; } = default!;
    public DbSet<DepenseCategorie> DepenseCategories { get; set; } = default!;
    public DbSet<ElementsFacture> ElementsFactures { get; set; } = default!;
    public DbSet<Employe> Employes { get; set; } = default!;
    public DbSet<EmployeBilanCredit> EmployeBilanCredits { get; set; } = default!;
    public DbSet<EmployeReglementCredit> EmployeReglementCredits { get; set; } = default!;
    public DbSet<EmployeCreditTransaction> EmployeCreditTransactions { get; set; } = default!;
    public DbSet<Facture> Factures { get; set; } = default!;
    public DbSet<Fournisseur> Fournisseurs { get; set; } = default!;
    public DbSet<IndicateursFinancier> IndicateursFinanciers { get; set; } = default!;
    public DbSet<Jaugeage> Jaugeages { get; set; } = default!;
    public DbSet<JaugeageDetail> JaugeageDetails { get; set; } = default!;
    public DbSet<JoursActivite> JoursActivites { get; set; } = default!;
    public DbSet<MoyensPaiement> MoyensPaiements { get; set; } = default!;
    public DbSet<Periode> Periodes { get; set; } = default!;
    public DbSet<PeriodeDetails> PeriodeDetails { get; set; } = default!;
    public DbSet<Pompe> Pompes { get; set; } = default!;
    public DbSet<Produit> Produits { get; set; } = default!;
    public DbSet<ReglementCredit> ReglementsCredit { get; set; } = default!;
    public DbSet<Reservoir> Reservoirs { get; set; } = default!;
    public DbSet<ReservoirCalibration> ReservoirCalibrations { get; set; } = default!;
    public DbSet<Service> Services { get; set; } = default!;
    public DbSet<VenteLubrifiantsEtArticles> VenteLubrifiantsEtArticles { get; set; } = default!;

    // Bon de Livraison entities
    public DbSet<BonLivraison> BonsLivraison { get; set; } = default!;
    public DbSet<BonLivraisonDetails> BonLivraisonDetails { get; set; } = default!;
    public DbSet<FactureBonLivraison> FactureBonLivraisons { get; set; } = default!;

    // Stock lot entities for FIFO inventory tracking
    public DbSet<StockLot> StockLots { get; set; } = default!;
    public DbSet<StockLotConsumption> StockLotConsumptions { get; set; } = default!;


    //seeding an admin user and super admin user
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity with PascalCase column names (matching your database)
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.UserId);
            
            // Map properties to PascalCase column names (PostgreSQL with quoted identifiers)
            entity.Property(e => e.UserId).HasColumnName("UserId");
            entity.Property(e => e.Name).HasColumnName("Name");
            entity.Property(e => e.Email).HasColumnName("Email");
            entity.Property(e => e.Password).HasColumnName("Password");
            entity.Property(e => e.IsAdmin).HasColumnName("IsAdmin");
            entity.Property(e => e.IsSuperAdmin).HasColumnName("IsSuperAdmin");
            entity.Property(e => e.IsActive).HasColumnName("IsActive");
            entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt");
            entity.Property(e => e.createdBy).HasColumnName("createdBy");
            entity.Property(e => e.updatedBy).HasColumnName("updatedBy");
            entity.Property(e => e.CanManageUsers).HasColumnName("CanManageUsers");
            entity.Property(e => e.CanManageProducts).HasColumnName("CanManageProducts");
            entity.Property(e => e.CanViewReports).HasColumnName("CanViewReports");
            entity.Property(e => e.CanManageSettings).HasColumnName("CanManageSettings");
            entity.Property(e => e.CanManageSales).HasColumnName("CanManageSales");
            entity.Property(e => e.CanManagePromotions).HasColumnName("CanManagePromotions");
            entity.Property(e => e.CanManageCustomers).HasColumnName("CanManageCustomers");
            entity.Property(e => e.CanManageSuppliers).HasColumnName("CanManageSuppliers");
            entity.Property(e => e.CanManageCategories).HasColumnName("CanManageCategories");
            entity.Property(e => e.ShowAcceuil).HasColumnName("ShowAcceuil");
            entity.Property(e => e.ShowTableauDeBord).HasColumnName("ShowTableauDeBord");
            entity.Property(e => e.ShowVente).HasColumnName("ShowVente");
            entity.Property(e => e.EditLivreur).HasColumnName("EditLivreur");
            entity.Property(e => e.AddAchat).HasColumnName("AddAchat");
            entity.Property(e => e.EditCiternes).HasColumnName("EditCiternes");
            entity.Property(e => e.EditPistolets).HasColumnName("EditPistolets");
            entity.Property(e => e.EditClients).HasColumnName("EditClients");
            entity.Property(e => e.AddBonFacturation).HasColumnName("AddBonFacturation");
            entity.Property(e => e.ShowDepenses).HasColumnName("ShowDepenses");
        });

        //Seed Admin User
        modelBuilder.Entity<User>().HasData(
            new User
            {
                UserId = 5,
                Name = "Admin User",
                Email = "admin@bombapromax.com",
                Password = "Admin@123", // In production, this should be hashed
                IsAdmin = true,
                IsSuperAdmin = false,
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                createdBy = 0,
                updatedBy = 0,
                // Admin permissions
                CanManageUsers = true,
                CanManageProducts = true,
                CanViewReports = true,
                CanManageSettings = true,
                CanManageSales = true,
                CanManagePromotions = true,
                CanManageCustomers = true,
                CanManageSuppliers = true,
                CanManageCategories = true,
                ShowAcceuil = true,
                ShowTableauDeBord = true,
                ShowVente = true,
                EditLivreur = true,
                AddAchat = true,
                EditCiternes = true,
                EditPistolets = true,
                EditClients = true,
                AddBonFacturation = true,
                ShowDepenses = true
            },  //SuperAdmin 1
            // Seed Super Admin User
            new User
            {
                UserId = 6,
                Name = "Super Admin",
                Email = "superadmin@bombapromax.com",
                Password = "SuperAdmin@123", // In production, this should be hashed
                IsAdmin = true,
                IsSuperAdmin = true,
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                createdBy = 0,
                updatedBy = 0,
                // Super Admin has all permissions
                CanManageUsers = true,
                CanManageProducts = true,
                CanViewReports = true,
                CanManageSettings = true,
                CanManageSales = true,
                CanManagePromotions = true,
                CanManageCustomers = true,
                CanManageSuppliers = true,
                CanManageCategories = true,
                ShowAcceuil = true,
                ShowTableauDeBord = true,
                ShowVente = true,
                EditLivreur = true,
                AddAchat = true,
                EditCiternes = true,
                EditPistolets = true,
                EditClients = true,
                AddBonFacturation = true,
                ShowDepenses = true
            }   //SuperAdmin 2
        );

        // Configure Client entity
        modelBuilder.Entity<Client>(entity =>
        {
            entity.ToTable("Clients");
            entity.HasKey(e => e.ID);

            // Map properties to PascalCase column names (like you did for User)
            entity.Property(e => e.ID).HasColumnName("ID");
            entity.Property(e => e.NumeroClient).HasColumnName("NumeroClient");
            entity.Property(e => e.Nom).HasColumnName("Nom");
            entity.Property(e => e.Contact).HasColumnName("Contact");
            entity.Property(e => e.CIN).HasColumnName("CIN");
            entity.Property(e => e.NomSociete).HasColumnName("NomSociete");
            entity.Property(e => e.userID).HasColumnName("userID");
            entity.Property(e => e.DateCreation).HasColumnName("DateCreation");
            entity.Property(e => e.DateModification).HasColumnName("DateModification");

            // Configure unique indexes
            entity.HasIndex(e => e.NumeroClient).IsUnique();
            entity.HasIndex(e => e.CIN).IsUnique();
        });

        // Configure BonLivraison entity
        modelBuilder.Entity<BonLivraison>(entity =>
        {
            entity.ToTable("BonsLivraison");
            entity.HasKey(e => e.ID);
            entity.HasIndex(e => e.NumeroBL).IsUnique();

            entity.HasOne(e => e.Client)
                .WithMany(c => c.BonsLivraison)
                .HasForeignKey(e => e.ClientID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure BonLivraisonDetails entity
        modelBuilder.Entity<BonLivraisonDetails>(entity =>
        {
            entity.ToTable("BonLivraisonDetails");
            entity.HasKey(e => e.ID);

            entity.HasOne(e => e.BonLivraison)
                .WithMany(b => b.Details)
                .HasForeignKey(e => e.BonLivraisonID)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Produit)
                .WithMany(p => p.BonLivraisonDetails)
                .HasForeignKey(e => e.ProduitID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Service)
                .WithMany(s => s.BonLivraisonDetails)
                .HasForeignKey(e => e.ServiceID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure FactureBonLivraison junction table
        modelBuilder.Entity<FactureBonLivraison>(entity =>
        {
            entity.ToTable("FactureBonLivraisons");
            entity.HasKey(e => e.ID);

            // A BL can only be linked to one Facture (enforces "invoiced once" rule)
            entity.HasIndex(e => e.BonLivraisonID).IsUnique();

            entity.HasOne(e => e.Facture)
                .WithMany(f => f.FactureBonLivraisons)
                .HasForeignKey(e => e.FactureID)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.BonLivraison)
                .WithMany(b => b.FactureBonLivraisons)
                .HasForeignKey(e => e.BonLivraisonID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure CreditTransaction to BonLivraison relationship
        modelBuilder.Entity<CreditTransaction>(entity =>
        {
            entity.HasOne(e => e.BonLivraison)
                .WithMany(b => b.CreditTransactions)
                .HasForeignKey(e => e.BonLivraisonID)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure ReservoirCalibration entity
        modelBuilder.Entity<ReservoirCalibration>(entity =>
        {
            entity.ToTable("ReservoirCalibrations");
            entity.HasKey(e => e.ID);

            // Unique constraint: one height entry per reservoir
            entity.HasIndex(e => new { e.ReservoirID, e.HauteurCm })
                  .IsUnique()
                  .HasDatabaseName("IX_ReservoirCalibrations_ReservoirID_HauteurCm");

            entity.HasOne(e => e.Reservoir)
                .WithMany(r => r.Calibrations)
                .HasForeignKey(e => e.ReservoirID)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure StockLot entity for FIFO inventory tracking
        modelBuilder.Entity<StockLot>(entity =>
        {
            entity.ToTable("StockLots");
            entity.HasKey(e => e.ID);

            // Index for FIFO queries (by reservoir, ordered by date)
            entity.HasIndex(e => new { e.ReservoirID, e.DateEntree })
                  .HasDatabaseName("IX_StockLots_ReservoirID_DateEntree");

            // Index for available stock queries
            entity.HasIndex(e => new { e.ReservoirID, e.Statut })
                  .HasDatabaseName("IX_StockLots_ReservoirID_Statut");

            entity.HasOne(e => e.Achat)
                .WithMany()
                .HasForeignKey(e => e.AchatID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Reservoir)
                .WithMany(r => r.StockLots)
                .HasForeignKey(e => e.ReservoirID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Produit)
                .WithMany()
                .HasForeignKey(e => e.ProduitID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure StockLotConsumption entity for audit trail
        modelBuilder.Entity<StockLotConsumption>(entity =>
        {
            entity.ToTable("StockLotConsumptions");
            entity.HasKey(e => e.ID);

            // Index for consumption queries by periode detail
            entity.HasIndex(e => e.PeriodeDetailID)
                  .HasDatabaseName("IX_StockLotConsumptions_PeriodeDetailID");

            entity.HasOne(e => e.StockLot)
                .WithMany(s => s.Consumptions)
                .HasForeignKey(e => e.StockLotID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.PeriodeDetail)
                .WithMany(p => p.StockLotConsumptions)
                .HasForeignKey(e => e.PeriodeDetailID)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql("Name=DefaultConnection");
        }
    }

}
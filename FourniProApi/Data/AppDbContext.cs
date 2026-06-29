using FourniProApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FourniProApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
    public DbSet<Produit> Produits => Set<Produit>();
    public DbSet<Fournisseur> Fournisseurs => Set<Fournisseur>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Chauffeur> Chauffeurs => Set<Chauffeur>();
    public DbSet<Camion> Camions => Set<Camion>();
    public DbSet<Citerne> Citernes => Set<Citerne>();
    public DbSet<Achat> Achats => Set<Achat>();
    public DbSet<Vente> Ventes => Set<Vente>();
    public DbSet<Credit> Credits => Set<Credit>();
    public DbSet<PaiementCredit> PaiementsCredit => Set<PaiementCredit>();
    public DbSet<CreditFournisseur> CreditsFournisseur => Set<CreditFournisseur>();
    public DbSet<PaiementFournisseur> PaiementsFournisseur => Set<PaiementFournisseur>();
    public DbSet<Avoir> Avoirs => Set<Avoir>();
    public DbSet<Employe> Employes => Set<Employe>();
    public DbSet<Voyage> Voyages => Set<Voyage>();
    public DbSet<StockVoyage> StockVoyages => Set<StockVoyage>();
    public DbSet<FraisVoyage> FraisVoyages => Set<FraisVoyage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserPermission>()
            .HasKey(up => new { up.UserId, up.PermissionId });

        modelBuilder.Entity<UserPermission>()
            .HasOne(up => up.User)
            .WithMany(u => u.UserPermissions)
            .HasForeignKey(up => up.UserId);

        modelBuilder.Entity<UserPermission>()
            .HasOne(up => up.Permission)
            .WithMany(p => p.UserPermissions)
            .HasForeignKey(up => up.PermissionId);

        modelBuilder.Entity<Permission>()
            .HasIndex(p => p.Name)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Vente>()
            .HasOne(v => v.Employe)
            .WithMany(e => e.Ventes)
            .HasForeignKey(v => v.EmployeId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Achat>()
            .HasOne(a => a.Employe)
            .WithMany(e => e.Achats)
            .HasForeignKey(a => a.EmployeId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Credit>()
            .HasOne(c => c.Employe)
            .WithMany(e => e.Credits)
            .HasForeignKey(c => c.EmployeId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Vente>()
            .HasOne(v => v.Voyage)
            .WithMany()
            .HasForeignKey(v => v.VoyageID)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Achat>()
            .HasOne(a => a.Voyage)
            .WithMany()
            .HasForeignKey(a => a.VoyageID)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Credit>()
            .HasOne(c => c.Voyage)
            .WithMany()
            .HasForeignKey(c => c.VoyageID)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<PaiementCredit>()
            .HasOne(p => p.Credit)
            .WithMany(c => c.PaiementsCredit)
            .HasForeignKey(p => p.CreditId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PaiementCredit>()
            .HasOne(p => p.Employe)
            .WithMany(e => e.PaiementsCredit)
            .HasForeignKey(p => p.EmployeId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Avoir>()
            .HasOne(a => a.Vente)
            .WithMany(v => v.Avoirs)
            .HasForeignKey(a => a.VenteId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Avoir>()
            .HasOne(a => a.Credit)
            .WithMany(c => c.Avoirs)
            .HasForeignKey(a => a.CreditId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Avoir>()
            .HasOne(a => a.Employe)
            .WithMany(e => e.Avoirs)
            .HasForeignKey(a => a.EmployeId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<CreditFournisseur>()
            .HasOne(c => c.Achat)
            .WithMany(a => a.CreditsFournisseur)
            .HasForeignKey(c => c.AchatId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<CreditFournisseur>()
            .HasOne(c => c.Fournisseur)
            .WithMany(f => f.CreditsFournisseur)
            .HasForeignKey(c => c.FournisseurId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<CreditFournisseur>()
            .HasOne(c => c.Employe)
            .WithMany(e => e.CreditsFournisseur)
            .HasForeignKey(c => c.EmployeId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<PaiementFournisseur>()
            .HasOne(p => p.CreditFournisseur)
            .WithMany(c => c.PaiementsFournisseur)
            .HasForeignKey(p => p.CreditFournisseurId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PaiementFournisseur>()
            .HasOne(p => p.Employe)
            .WithMany(e => e.PaiementsFournisseur)
            .HasForeignKey(p => p.EmployeId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<StockVoyage>()
            .HasOne(s => s.Voyage)
            .WithMany(v => v.Stocks)
            .HasForeignKey(s => s.VoyageId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<FraisVoyage>()
            .HasOne(f => f.Voyage)
            .WithMany(v => v.Frais)
            .HasForeignKey(f => f.VoyageId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Voyage>()
            .HasOne(v => v.Camion)
            .WithMany()
            .HasForeignKey(v => v.CamionId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Voyage>()
            .HasOne(v => v.Chauffeur)
            .WithMany()
            .HasForeignKey(v => v.ChauffeurId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Voyage>()
            .HasOne(v => v.Citerne)
            .WithMany()
            .HasForeignKey(v => v.CiterneId)
            .OnDelete(DeleteBehavior.SetNull);

        // Seed data
        modelBuilder.Entity<User>().HasData(new User
        {
            UserId = 1,
            Name = "admin",
            Email = "admin",
            Password = "1234",
            IsAdmin = true,
            IsSuperAdmin = true,
            IsActive = true,
            CreatedBy = 1,
            UpdatedBy = 1,
            CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        });
    }
}

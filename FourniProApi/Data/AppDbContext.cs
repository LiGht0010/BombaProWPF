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

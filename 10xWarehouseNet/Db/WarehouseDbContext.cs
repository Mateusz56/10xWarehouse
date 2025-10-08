using _10xWarehouseNet.Db.Models;
using Microsoft.EntityFrameworkCore;

namespace _10xWarehouseNet.Db;

public class WarehouseDbContext(DbContextOptions<WarehouseDbContext> options) : DbContext(options)
{
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<OrganizationMember> OrganizationMembers { get; set; }
    public DbSet<Invitation> Invitations { get; set; }
    public DbSet<Warehouse> Warehouses { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<ProductTemplate> ProductTemplates { get; set; }
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<StockMovement> StockMovements { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("app");

        modelBuilder.Entity<OrganizationMember>()
            .HasKey(om => new { om.OrganizationId, om.UserId });

        modelBuilder.Entity<Invitation>()
            .HasIndex(i => i.Token)
            .IsUnique();
            
        modelBuilder.Entity<ProductTemplate>()
            .HasIndex(pt => new { pt.OrganizationId, pt.Barcode })
            .IsUnique();

        modelBuilder.Entity<Inventory>()
            .HasIndex(i => new { i.OrganizationId, i.ProductTemplateId, i.LocationId })
            .IsUnique();

        modelBuilder.Entity<StockMovement>()
            .HasOne(sm => sm.FromLocation)
            .WithMany()
            .HasForeignKey(sm => sm.FromLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StockMovement>()
            .HasOne(sm => sm.ToLocation)
            .WithMany()
            .HasForeignKey(sm => sm.ToLocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

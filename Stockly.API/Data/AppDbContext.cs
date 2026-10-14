using Microsoft.EntityFrameworkCore;
using Stockly.API.Models;

namespace Stockly.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<Product>()
            .HasIndex(p => new { p.UserId, p.Sku })
            .IsUnique();

        modelBuilder.Entity<Product>().Property(p => p.CostPrice).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Product>().Property(p => p.SalePrice).HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Category>()
            .HasOne(c => c.User).WithMany(u => u.Categories)
            .HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category).WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Product>()
            .HasOne(p => p.User).WithMany(u => u.Products)
            .HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StockMovement>()
            .HasOne(m => m.Product).WithMany(p => p.Movements)
            .HasForeignKey(m => m.ProductId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StockMovement>()
            .HasOne(m => m.User).WithMany(u => u.Movements)
            .HasForeignKey(m => m.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}
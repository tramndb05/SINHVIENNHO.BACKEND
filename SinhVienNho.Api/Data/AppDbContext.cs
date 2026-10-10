using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SinhVienNho.Api.Entities;

namespace SinhVienNho.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Chart> Charts => Set<Chart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<PurchasedChart> PurchasedCharts => Set<PurchasedChart>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var stringListConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<List<string>, string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
        );

        var stringListComparer = new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>>(
            (a, b) => (a ?? new()).SequenceEqual(b ?? new()),
            v => v.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())),
            v => v.ToList()
        );

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Phone).IsUnique();
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(c => new { c.Slug, c.Type }).IsUnique();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(p => p.Slug).IsUnique();
            entity.Property(p => p.Details).HasConversion(stringListConverter).Metadata.SetValueComparer(stringListComparer);
            entity.HasOne(p => p.Category).WithMany(c => c.Products).HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Chart>(entity =>
        {
            entity.HasIndex(c => c.Slug).IsUnique();
            entity.Property(c => c.FullChartImages).HasConversion(stringListConverter).Metadata.SetValueComparer(stringListComparer);
            entity.Property(c => c.Includes).HasConversion(stringListConverter).Metadata.SetValueComparer(stringListComparer);
            entity.HasOne(c => c.Category).WithMany(cat => cat.Charts).HasForeignKey(c => c.CategoryId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasIndex(ci => new { ci.UserId, ci.ProductId }).IsUnique();
            entity.HasOne(ci => ci.User).WithMany(u => u.CartItems).HasForeignKey(ci => ci.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(ci => ci.Product).WithMany(p => p.CartItems).HasForeignKey(ci => ci.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasOne(o => o.User).WithMany(u => u.Orders).HasForeignKey(o => o.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasOne(oi => oi.Order).WithMany(o => o.Items).HasForeignKey(oi => oi.OrderId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(oi => oi.Product).WithMany(p => p.OrderItems).HasForeignKey(oi => oi.ProductId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(oi => oi.Chart).WithMany(c => c.OrderItems).HasForeignKey(oi => oi.ChartId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PurchasedChart>(entity =>
        {
            entity.HasIndex(pc => new { pc.UserId, pc.ChartId }).IsUnique();
            entity.HasOne(pc => pc.User).WithMany(u => u.PurchasedCharts).HasForeignKey(pc => pc.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(pc => pc.Chart).WithMany(c => c.PurchasedCharts).HasForeignKey(pc => pc.ChartId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}

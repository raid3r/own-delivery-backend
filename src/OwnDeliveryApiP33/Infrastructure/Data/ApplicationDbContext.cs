using Microsoft.EntityFrameworkCore;
using OwnDeliveryApiP33.Domain.Entities;

namespace OwnDeliveryApiP33.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Courier> Couriers => Set<Courier>();
    public DbSet<Administrator> Administrators => Set<Administrator>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<CourierLocation> CourierLocations => Set<CourierLocation>();
    public DbSet<CourierDocument> CourierDocuments => Set<CourierDocument>();
    public DbSet<Tariff> Tariffs => Set<Tariff>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
    public DbSet<Rating> Ratings => Set<Rating>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.PasswordHash).IsRequired();
        });

        // Courier Configuration
        modelBuilder.Entity<Courier>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                .WithOne()
                .HasForeignKey<Courier>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.LicenseNumber).HasMaxLength(50);
            entity.HasMany(e => e.Orders)
                .WithOne(o => o.Courier)
                .HasForeignKey(o => o.CourierId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasMany(e => e.Locations)
                .WithOne(l => l.Courier)
                .HasForeignKey(l => l.CourierId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Documents)
                .WithOne(d => d.Courier)
                .HasForeignKey(d => d.CourierId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Ratings)
                .WithOne(r => r.Courier)
                .HasForeignKey(r => r.CourierId)
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasIndex(e => e.UserId).IsUnique();
        });

        // Customer Configuration
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                .WithOne()
                .HasForeignKey<Customer>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Orders)
                .WithOne(o => o.Customer)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.UserId).IsUnique();
        });

        // Administrator Configuration
        modelBuilder.Entity<Administrator>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                .WithOne()
                .HasForeignKey<Administrator>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.UserId).IsUnique();
        });

        // Order Configuration
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.OrderNumber).IsUnique();
            entity.HasOne(e => e.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Tariff)
                .WithMany(t => t.Orders)
                .HasForeignKey(e => e.TariffId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.OwnsOne(e => e.PickupAddress);
            entity.OwnsOne(e => e.DeliveryAddress);
            entity.OwnsOne(e => e.Dimensions);
            entity.HasMany(e => e.StatusHistory)
                .WithOne(sh => sh.Order)
                .HasForeignKey(sh => sh.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Ratings)
                .WithOne(r => r.Order)
                .HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.Payment)
                .WithOne(p => p.Order)
                .HasForeignKey<Payment>(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.CourierId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
        });

        // Tariff Configuration
        modelBuilder.Entity<Tariff>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.OwnsOne(e => e.MaxDimensions);
            entity.HasIndex(e => e.IsActive);
        });

        // CourierLocation Configuration
        modelBuilder.Entity<CourierLocation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Courier)
                .WithMany(c => c.Locations)
                .HasForeignKey(e => e.CourierId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.OwnsOne(e => e.Location);
            entity.HasIndex(e => e.CourierId);
            entity.HasIndex(e => e.Timestamp);
        });

        // CourierDocument Configuration
        modelBuilder.Entity<CourierDocument>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Courier)
                .WithMany(c => c.Documents)
                .HasForeignKey(e => e.CourierId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.VerifiedByUser)
                .WithMany()
                .HasForeignKey(e => e.VerifiedBy)
                .OnDelete(DeleteBehavior.NoAction);
            entity.Property(e => e.DocumentNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DocumentUrl).IsRequired();
            entity.HasIndex(e => e.CourierId);
            entity.HasIndex(e => e.DocumentType);
            entity.HasIndex(e => e.Status);
        });

        // OrderStatusHistory Configuration
        modelBuilder.Entity<OrderStatusHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Order)
                .WithMany(o => o.StatusHistory)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.ChangedByUser)
                .WithMany()
                .HasForeignKey(e => e.ChangedBy)
                .OnDelete(DeleteBehavior.Restrict);
            entity.OwnsOne(e => e.Location);
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.Timestamp);
        });

        // Rating Configuration
        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Order)
                .WithMany(o => o.Ratings)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.Courier)
                .WithMany(c => c.Ratings)
                .HasForeignKey(e => e.CourierId)
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.Customer)
                .WithMany()
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasIndex(e => e.CourierId);
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.CreatedAt);
        });

        // Payment Configuration
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Order)
                .WithOne(o => o.Payment)
                .HasForeignKey<Payment>(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.TransactionId).HasMaxLength(100);
            entity.HasIndex(e => e.TransactionId).IsUnique();
        });

        // Notification Configuration
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.IsRead);
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}

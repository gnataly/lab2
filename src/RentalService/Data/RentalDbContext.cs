using Microsoft.EntityFrameworkCore;
using RentalService.Models;

namespace RentalService.Data;

public class RentalDbContext : DbContext
{
    public RentalDbContext(DbContextOptions<RentalDbContext> options) : base(options)
    {
    }

    public DbSet<Rental> Rentals => Set<Rental>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Rental>(entity =>
        {
            entity.ToTable("rental");
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(r => r.RentalUid).HasColumnName("rental_uid").IsRequired();
            entity.Property(r => r.Username).HasColumnName("username").HasMaxLength(80).IsRequired();
            entity.Property(r => r.PaymentUid).HasColumnName("payment_uid").IsRequired();
            entity.Property(r => r.CarUid).HasColumnName("car_uid").IsRequired();
            entity.Property(r => r.DateFrom).HasColumnName("date_from").IsRequired();
            entity.Property(r => r.DateTo).HasColumnName("date_to").IsRequired();
            entity.Property(r => r.Status).HasColumnName("status").HasMaxLength(20).IsRequired();

            entity.HasIndex(r => r.RentalUid).IsUnique();
        });
    }
}
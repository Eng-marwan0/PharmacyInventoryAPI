using Microsoft.EntityFrameworkCore;
using PharmacyInventoryAPI.Models;

namespace PharmacyInventoryAPI.Data;

public class PharmacyDbContext : DbContext
{
    public PharmacyDbContext(DbContextOptions<PharmacyDbContext> options) : base(options) { }

    public DbSet<Medication> Medications => Set<Medication>();
    public DbSet<Batch> Batches => Set<Batch>();
    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Medication>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Name).IsRequired().HasMaxLength(200);
            entity.Property(m => m.GenericName).HasMaxLength(200);
            entity.Property(m => m.Manufacturer).HasMaxLength(200);
            entity.Property(m => m.DosageForm).HasMaxLength(100);
            entity.Property(m => m.Strength).HasMaxLength(100);
            entity.Property(m => m.Category).HasMaxLength(100);
            entity.Property(m => m.Description).HasMaxLength(1000);
            entity.HasIndex(m => m.Name);
            entity.HasIndex(m => m.Category);
        });

        modelBuilder.Entity<Batch>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.BatchNumber).IsRequired().HasMaxLength(100);
            entity.Property(b => b.Supplier).HasMaxLength(200);
            entity.Property(b => b.UnitPrice).HasPrecision(18, 2);
            entity.HasIndex(b => b.BatchNumber).IsUnique();
            entity.HasIndex(b => b.ExpiryDate);
            entity.HasIndex(b => b.Status);

            entity.HasOne(b => b.Medication)
                .WithMany(m => m.Batches)
                .HasForeignKey(b => b.MedicationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StockTransaction>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Reference).HasMaxLength(200);
            entity.Property(t => t.Notes).HasMaxLength(500);
            entity.HasIndex(t => t.TransactionDate);

            entity.HasOne(t => t.Batch)
                .WithMany()
                .HasForeignKey(t => t.BatchId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

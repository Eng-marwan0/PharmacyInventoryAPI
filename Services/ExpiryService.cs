using Microsoft.EntityFrameworkCore;
using PharmacyInventoryAPI.Data;
using PharmacyInventoryAPI.DTOs;
using PharmacyInventoryAPI.Models;

namespace PharmacyInventoryAPI.Services;

public class ExpiryService : IExpiryService
{
    private readonly PharmacyDbContext _context;

    public ExpiryService(PharmacyDbContext context)
    {
        _context = context;
    }

    public async Task<ExpiryReportDto> GetExpiryReportAsync(int warningDays = 90, int criticalDays = 30)
    {
        if (warningDays < criticalDays)
            throw new ArgumentException($"warningDays ({warningDays}) must be >= criticalDays ({criticalDays}).");

        var now = DateTime.UtcNow.Date;
        var criticalDate = now.AddDays(criticalDays);
        var warningDate = now.AddDays(warningDays);

        var batches = await _context.Batches
            .Include(b => b.Medication)
            .Where(b => b.Status == BatchStatus.Active || b.Status == BatchStatus.Expired)
            .Where(b => b.Quantity > 0)
            .Where(b => b.ExpiryDate.Date <= warningDate)
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync();

        var expired = batches.Where(b => b.ExpiryDate.Date < now).ToList();
        var critical = batches.Where(b => b.ExpiryDate.Date >= now && b.ExpiryDate.Date <= criticalDate).ToList();
        var warning = batches.Where(b => b.ExpiryDate.Date > criticalDate && b.ExpiryDate.Date <= warningDate).ToList();

        return new ExpiryReportDto(
            expired.Select(b => MapToAlert(b, "Expired")).ToList(),
            critical.Select(b => MapToAlert(b, "Critical")).ToList(),
            warning.Select(b => MapToAlert(b, "Warning")).ToList(),
            expired.Sum(b => b.Quantity),
            critical.Sum(b => b.Quantity) + warning.Sum(b => b.Quantity),
            DateTime.UtcNow
        );
    }

    public async Task<IEnumerable<ExpiryAlertDto>> GetExpiringBatchesAsync(int days = 90)
    {
        var now = DateTime.UtcNow.Date;
        var targetDate = now.AddDays(days);

        var batches = await _context.Batches
            .Include(b => b.Medication)
            .Where(b => b.Status == BatchStatus.Active)
            .Where(b => b.Quantity > 0)
            .Where(b => b.ExpiryDate.Date >= now && b.ExpiryDate.Date <= targetDate)
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync();

        return batches.Select(b =>
        {
            var daysUntilExpiry = (b.ExpiryDate.Date - now).Days;
            var alertLevel = daysUntilExpiry <= 30 ? "Critical" : "Warning";
            return MapToAlert(b, alertLevel);
        });
    }

    public async Task<IEnumerable<ExpiryAlertDto>> GetExpiredBatchesAsync()
    {
        var now = DateTime.UtcNow.Date;

        var batches = await _context.Batches
            .Include(b => b.Medication)
            .Where(b => b.ExpiryDate.Date < now)
            .Where(b => b.Quantity > 0)
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync();

        return batches.Select(b => MapToAlert(b, "Expired"));
    }

    public async Task<int> MarkExpiredBatchesAsync()
    {
        var now = DateTime.UtcNow.Date;

        var expiredBatches = await _context.Batches
            .Where(b => b.Status == BatchStatus.Active)
            .Where(b => b.ExpiryDate.Date < now)
            .ToListAsync();

        foreach (var batch in expiredBatches)
        {
            batch.Status = BatchStatus.Expired;
            batch.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return expiredBatches.Count;
    }

    public async Task<InventorySummaryDto> GetInventorySummaryAsync()
    {
        var now = DateTime.UtcNow.Date;
        var nearExpiryDate = now.AddDays(90);

        var medications = await _context.Medications
            .Include(m => m.Batches)
            .ToListAsync();

        var allBatches = await _context.Batches.ToListAsync();
        var activeBatches = allBatches.Where(b => b.Status == BatchStatus.Active).ToList();

        var totalStock = activeBatches.Sum(b => b.Quantity);
        var expiredCount = allBatches.Count(b => b.ExpiryDate.Date < now && b.Quantity > 0);
        var nearExpiryCount = activeBatches.Count(b => b.ExpiryDate.Date >= now && b.ExpiryDate.Date <= nearExpiryDate);

        var lowStockCount = medications.Count(m =>
        {
            var stock = m.Batches
                .Where(b => b.Status == BatchStatus.Active)
                .Sum(b => b.Quantity);
            return stock <= m.ReorderLevel;
        });

        var totalValue = activeBatches.Sum(b => b.Quantity * b.UnitPrice);

        return new InventorySummaryDto(
            medications.Count,
            allBatches.Count,
            activeBatches.Count,
            totalStock,
            expiredCount,
            nearExpiryCount,
            lowStockCount,
            totalValue,
            DateTime.UtcNow
        );
    }

    private static ExpiryAlertDto MapToAlert(Batch batch, string alertLevel)
    {
        var daysUntilExpiry = (batch.ExpiryDate.Date - DateTime.UtcNow.Date).Days;
        return new ExpiryAlertDto(
            batch.Id,
            batch.BatchNumber,
            batch.MedicationId,
            batch.Medication.Name,
            batch.Medication.Strength,
            batch.Quantity,
            batch.ExpiryDate,
            daysUntilExpiry,
            alertLevel
        );
    }
}

using Microsoft.EntityFrameworkCore;
using PharmacyInventoryAPI.Data;
using PharmacyInventoryAPI.DTOs;
using PharmacyInventoryAPI.Models;

namespace PharmacyInventoryAPI.Services;

public class BatchService : IBatchService
{
    private readonly PharmacyDbContext _context;

    public BatchService(PharmacyDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<BatchDto>> GetAllAsync(int? medicationId, string? status)
    {
        var query = _context.Batches
            .Include(b => b.Medication)
            .AsQueryable();

        if (medicationId.HasValue)
        {
            query = query.Where(b => b.MedicationId == medicationId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BatchStatus>(status, true, out var batchStatus))
        {
            query = query.Where(b => b.Status == batchStatus);
        }

        var batches = await query.OrderByDescending(b => b.CreatedAt).ToListAsync();

        return batches.Select(b => MapToDto(b));
    }

    public async Task<BatchDto?> GetByIdAsync(int id)
    {
        var batch = await _context.Batches
            .Include(b => b.Medication)
            .FirstOrDefaultAsync(b => b.Id == id);

        return batch == null ? null : MapToDto(batch);
    }

    public async Task<BatchDto?> GetByBatchNumberAsync(string batchNumber)
    {
        var batch = await _context.Batches
            .Include(b => b.Medication)
            .FirstOrDefaultAsync(b => b.BatchNumber == batchNumber);

        return batch == null ? null : MapToDto(batch);
    }

    public async Task<BatchDto> CreateAsync(CreateBatchDto dto)
    {
        var medication = await _context.Medications.FindAsync(dto.MedicationId);
        if (medication == null)
        {
            throw new KeyNotFoundException($"Medication with ID {dto.MedicationId} not found.");
        }

        var existingBatch = await _context.Batches
            .AnyAsync(b => b.BatchNumber == dto.BatchNumber);
        if (existingBatch)
        {
            throw new InvalidOperationException($"Batch number '{dto.BatchNumber}' already exists.");
        }

        var batch = new Batch
        {
            BatchNumber = dto.BatchNumber,
            MedicationId = dto.MedicationId,
            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice,
            ManufactureDate = dto.ManufactureDate,
            ExpiryDate = dto.ExpiryDate,
            Supplier = dto.Supplier,
            Status = dto.ExpiryDate.Date < DateTime.UtcNow.Date ? BatchStatus.Expired : BatchStatus.Active,
            ReceivedDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Batches.Add(batch);

        var transaction = new StockTransaction
        {
            Batch = batch,
            Type = TransactionType.Received,
            Quantity = dto.Quantity,
            Reference = $"Initial receipt - Batch {dto.BatchNumber}",
            Notes = $"Batch received from {dto.Supplier}",
            TransactionDate = DateTime.UtcNow
        };

        _context.StockTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        batch.Medication = medication;
        return MapToDto(batch);
    }

    public async Task<BatchDto?> UpdateAsync(int id, UpdateBatchDto dto)
    {
        var batch = await _context.Batches
            .Include(b => b.Medication)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (batch == null) return null;

        batch.Quantity = dto.Quantity;
        batch.UnitPrice = dto.UnitPrice;
        batch.Supplier = dto.Supplier;
        batch.Status = dto.Status;
        batch.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(batch);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var batch = await _context.Batches.FindAsync(id);
        if (batch == null) return false;

        if (batch.Quantity > 0)
        {
            throw new InvalidOperationException(
                "Cannot delete a batch with remaining stock. Deplete or adjust quantity first.");
        }

        var transactions = await _context.StockTransactions.Where(t => t.BatchId == id).ToListAsync();
        _context.StockTransactions.RemoveRange(transactions);
        _context.Batches.Remove(batch);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<StockTransactionDto> RecordTransactionAsync(CreateStockTransactionDto dto)
    {
        var batch = await _context.Batches
            .Include(b => b.Medication)
            .FirstOrDefaultAsync(b => b.Id == dto.BatchId);

        if (batch == null)
        {
            throw new KeyNotFoundException($"Batch with ID {dto.BatchId} not found.");
        }

        if (batch.Status == BatchStatus.Expired)
        {
            throw new InvalidOperationException("Cannot perform transactions on expired batches.");
        }

        switch (dto.Type)
        {
            case TransactionType.Dispensed:
            case TransactionType.Disposed:
            case TransactionType.Transferred:
                if (batch.Quantity < dto.Quantity)
                {
                    throw new InvalidOperationException(
                        $"Insufficient stock. Available: {batch.Quantity}, Requested: {dto.Quantity}");
                }
                batch.Quantity -= dto.Quantity;
                if (batch.Quantity == 0 && batch.Status == BatchStatus.Active) batch.Status = BatchStatus.Depleted;
                break;

            case TransactionType.Received:
            case TransactionType.Returned:
                batch.Quantity += dto.Quantity;
                if (batch.Status == BatchStatus.Depleted) batch.Status = BatchStatus.Active;
                break;

            case TransactionType.Adjusted:
                batch.Quantity = dto.Quantity;
                if (batch.Status == BatchStatus.Depleted && dto.Quantity > 0) batch.Status = BatchStatus.Active;
                break;
        }

        batch.UpdatedAt = DateTime.UtcNow;

        var transaction = new StockTransaction
        {
            BatchId = dto.BatchId,
            Type = dto.Type,
            Quantity = dto.Quantity,
            Reference = dto.Reference,
            Notes = dto.Notes,
            TransactionDate = DateTime.UtcNow
        };

        _context.StockTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        return new StockTransactionDto(
            transaction.Id,
            transaction.BatchId,
            batch.BatchNumber,
            batch.Medication.Name,
            transaction.Type.ToString(),
            transaction.Quantity,
            transaction.Reference,
            transaction.Notes,
            transaction.TransactionDate
        );
    }

    public async Task<IEnumerable<StockTransactionDto>> GetTransactionsAsync(
        int? batchId, DateTime? from, DateTime? to)
    {
        var query = _context.StockTransactions
            .Include(t => t.Batch)
            .ThenInclude(b => b.Medication)
            .AsQueryable();

        if (batchId.HasValue)
        {
            query = query.Where(t => t.BatchId == batchId.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(t => t.TransactionDate >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(t => t.TransactionDate <= to.Value);
        }

        var transactions = await query.OrderByDescending(t => t.TransactionDate).ToListAsync();

        return transactions.Select(t => new StockTransactionDto(
            t.Id,
            t.BatchId,
            t.Batch.BatchNumber,
            t.Batch.Medication.Name,
            t.Type.ToString(),
            t.Quantity,
            t.Reference,
            t.Notes,
            t.TransactionDate
        ));
    }

    private static BatchDto MapToDto(Batch batch)
    {
        var daysUntilExpiry = (batch.ExpiryDate.Date - DateTime.UtcNow.Date).Days;
        return new BatchDto(
            batch.Id,
            batch.BatchNumber,
            batch.MedicationId,
            batch.Medication.Name,
            batch.Quantity,
            batch.UnitPrice,
            batch.ManufactureDate,
            batch.ExpiryDate,
            batch.Supplier,
            batch.Status.ToString(),
            daysUntilExpiry,
            daysUntilExpiry < 0,
            daysUntilExpiry >= 0 && daysUntilExpiry <= 90,
            batch.ReceivedDate,
            batch.CreatedAt
        );
    }
}

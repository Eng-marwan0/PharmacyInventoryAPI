using Microsoft.EntityFrameworkCore;
using PharmacyInventoryAPI.Data;
using PharmacyInventoryAPI.DTOs;
using PharmacyInventoryAPI.Models;

namespace PharmacyInventoryAPI.Services;

public class MedicationService : IMedicationService
{
    private readonly PharmacyDbContext _context;

    public MedicationService(PharmacyDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MedicationDto>> GetAllAsync(string? search, string? category)
    {
        var query = _context.Medications
            .Include(m => m.Batches)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(m =>
                m.Name.ToLower().Contains(searchLower) ||
                m.GenericName.ToLower().Contains(searchLower) ||
                m.Manufacturer.ToLower().Contains(searchLower));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(m => m.Category.ToLower() == category.ToLower());
        }

        var medications = await query.OrderBy(m => m.Name).ToListAsync();

        return medications.Select(MapToDto);
    }

    public async Task<MedicationDetailDto?> GetByIdAsync(int id)
    {
        var medication = await _context.Medications
            .Include(m => m.Batches)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (medication == null) return null;

        return new MedicationDetailDto(
            medication.Id,
            medication.Name,
            medication.GenericName,
            medication.Manufacturer,
            medication.DosageForm,
            medication.Strength,
            medication.Category,
            medication.Description,
            medication.ReorderLevel,
            medication.Batches.Where(b => b.Status == BatchStatus.Active).Sum(b => b.Quantity),
            medication.CreatedAt,
            medication.UpdatedAt,
            medication.Batches.Select(b => MapBatchToDto(b, medication.Name)).ToList()
        );
    }

    public async Task<MedicationDto> CreateAsync(CreateMedicationDto dto)
    {
        var medication = new Medication
        {
            Name = dto.Name,
            GenericName = dto.GenericName,
            Manufacturer = dto.Manufacturer,
            DosageForm = dto.DosageForm,
            Strength = dto.Strength,
            Category = dto.Category,
            Description = dto.Description,
            ReorderLevel = dto.ReorderLevel,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Medications.Add(medication);
        await _context.SaveChangesAsync();

        return MapToDto(medication);
    }

    public async Task<MedicationDto?> UpdateAsync(int id, UpdateMedicationDto dto)
    {
        var medication = await _context.Medications
            .Include(m => m.Batches)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (medication == null) return null;

        medication.Name = dto.Name;
        medication.GenericName = dto.GenericName;
        medication.Manufacturer = dto.Manufacturer;
        medication.DosageForm = dto.DosageForm;
        medication.Strength = dto.Strength;
        medication.Category = dto.Category;
        medication.Description = dto.Description;
        medication.ReorderLevel = dto.ReorderLevel;
        medication.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(medication);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var medication = await _context.Medications
            .Include(m => m.Batches)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (medication == null) return false;

        if (medication.Batches.Any(b => b.Status == BatchStatus.Active && b.Quantity > 0))
        {
            throw new InvalidOperationException(
                "Cannot delete medication with active stock. Dispose or transfer batches first.");
        }

        _context.Medications.Remove(medication);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<MedicationDto>> GetLowStockAsync()
    {
        var medications = await _context.Medications
            .Include(m => m.Batches)
            .ToListAsync();

        return medications
            .Where(m =>
            {
                var totalStock = m.Batches
                    .Where(b => b.Status == BatchStatus.Active)
                    .Sum(b => b.Quantity);
                return totalStock <= m.ReorderLevel;
            })
            .Select(MapToDto);
    }

    private static MedicationDto MapToDto(Medication medication)
    {
        return new MedicationDto(
            medication.Id,
            medication.Name,
            medication.GenericName,
            medication.Manufacturer,
            medication.DosageForm,
            medication.Strength,
            medication.Category,
            medication.Description,
            medication.ReorderLevel,
            medication.Batches?.Where(b => b.Status == BatchStatus.Active).Sum(b => b.Quantity) ?? 0,
            medication.CreatedAt,
            medication.UpdatedAt
        );
    }

    private static BatchDto MapBatchToDto(Batch batch, string medicationName)
    {
        var daysUntilExpiry = (batch.ExpiryDate.Date - DateTime.UtcNow.Date).Days;
        return new BatchDto(
            batch.Id,
            batch.BatchNumber,
            batch.MedicationId,
            medicationName,
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

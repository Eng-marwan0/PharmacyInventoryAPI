using PharmacyInventoryAPI.DTOs;

namespace PharmacyInventoryAPI.Services;

public interface IMedicationService
{
    Task<IEnumerable<MedicationDto>> GetAllAsync(string? search, string? category);
    Task<MedicationDetailDto?> GetByIdAsync(int id);
    Task<MedicationDto> CreateAsync(CreateMedicationDto dto);
    Task<MedicationDto?> UpdateAsync(int id, UpdateMedicationDto dto);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<MedicationDto>> GetLowStockAsync();
}

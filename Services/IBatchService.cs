using PharmacyInventoryAPI.DTOs;

namespace PharmacyInventoryAPI.Services;

public interface IBatchService
{
    Task<IEnumerable<BatchDto>> GetAllAsync(int? medicationId, string? status);
    Task<BatchDto?> GetByIdAsync(int id);
    Task<BatchDto?> GetByBatchNumberAsync(string batchNumber);
    Task<BatchDto> CreateAsync(CreateBatchDto dto);
    Task<BatchDto?> UpdateAsync(int id, UpdateBatchDto dto);
    Task<bool> DeleteAsync(int id);
    Task<StockTransactionDto> RecordTransactionAsync(CreateStockTransactionDto dto);
    Task<IEnumerable<StockTransactionDto>> GetTransactionsAsync(int? batchId, DateTime? from, DateTime? to);
}

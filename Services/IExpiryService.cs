using PharmacyInventoryAPI.DTOs;

namespace PharmacyInventoryAPI.Services;

public interface IExpiryService
{
    Task<ExpiryReportDto> GetExpiryReportAsync(int warningDays = 90, int criticalDays = 30);
    Task<IEnumerable<ExpiryAlertDto>> GetExpiringBatchesAsync(int days = 90);
    Task<IEnumerable<ExpiryAlertDto>> GetExpiredBatchesAsync();
    Task<int> MarkExpiredBatchesAsync();
    Task<InventorySummaryDto> GetInventorySummaryAsync();
}

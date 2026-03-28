namespace PharmacyInventoryAPI.DTOs;

public record ExpiryAlertDto(
    int BatchId,
    string BatchNumber,
    int MedicationId,
    string MedicationName,
    string Strength,
    int Quantity,
    DateTime ExpiryDate,
    int DaysUntilExpiry,
    string AlertLevel
);

public record ExpiryReportDto(
    List<ExpiryAlertDto> ExpiredBatches,
    List<ExpiryAlertDto> CriticalBatches,
    List<ExpiryAlertDto> WarningBatches,
    int TotalExpiredQuantity,
    int TotalAtRiskQuantity,
    DateTime GeneratedAt
);

public record InventorySummaryDto(
    int TotalMedications,
    int TotalBatches,
    int TotalActiveBatches,
    int TotalStock,
    int ExpiredBatchCount,
    int NearExpiryBatchCount,
    int LowStockMedicationCount,
    decimal TotalInventoryValue,
    DateTime GeneratedAt
);

using PharmacyInventoryAPI.Models;

namespace PharmacyInventoryAPI.DTOs;

public record CreateBatchDto(
    string BatchNumber,
    int MedicationId,
    int Quantity,
    decimal UnitPrice,
    DateTime ManufactureDate,
    DateTime ExpiryDate,
    string Supplier
);

public record UpdateBatchDto(
    int Quantity,
    decimal UnitPrice,
    string Supplier,
    BatchStatus Status
);

public record BatchDto(
    int Id,
    string BatchNumber,
    int MedicationId,
    string MedicationName,
    int Quantity,
    decimal UnitPrice,
    DateTime ManufactureDate,
    DateTime ExpiryDate,
    string Supplier,
    string Status,
    int DaysUntilExpiry,
    bool IsExpired,
    bool IsNearExpiry,
    DateTime ReceivedDate,
    DateTime CreatedAt
);

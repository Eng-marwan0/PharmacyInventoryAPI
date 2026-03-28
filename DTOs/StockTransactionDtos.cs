using PharmacyInventoryAPI.Models;

namespace PharmacyInventoryAPI.DTOs;

public record CreateStockTransactionDto(
    int BatchId,
    TransactionType Type,
    int Quantity,
    string Reference,
    string Notes
);

public record StockTransactionDto(
    int Id,
    int BatchId,
    string BatchNumber,
    string MedicationName,
    string Type,
    int Quantity,
    string Reference,
    string Notes,
    DateTime TransactionDate
);

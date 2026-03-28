namespace PharmacyInventoryAPI.DTOs;

public record CreateMedicationDto(
    string Name,
    string GenericName,
    string Manufacturer,
    string DosageForm,
    string Strength,
    string Category,
    string Description,
    int ReorderLevel
);

public record UpdateMedicationDto(
    string Name,
    string GenericName,
    string Manufacturer,
    string DosageForm,
    string Strength,
    string Category,
    string Description,
    int ReorderLevel
);

public record MedicationDto(
    int Id,
    string Name,
    string GenericName,
    string Manufacturer,
    string DosageForm,
    string Strength,
    string Category,
    string Description,
    int ReorderLevel,
    int TotalStock,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record MedicationDetailDto(
    int Id,
    string Name,
    string GenericName,
    string Manufacturer,
    string DosageForm,
    string Strength,
    string Category,
    string Description,
    int ReorderLevel,
    int TotalStock,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<BatchDto> Batches
);

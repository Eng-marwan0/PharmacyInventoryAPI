namespace PharmacyInventoryAPI.Models;

public class Batch
{
    public int Id { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public int MedicationId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public DateTime ManufactureDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string Supplier { get; set; } = string.Empty;
    public BatchStatus Status { get; set; } = BatchStatus.Active;
    public DateTime ReceivedDate { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Medication Medication { get; set; } = null!;
}

public enum BatchStatus
{
    Active,
    Expired,
    Recalled,
    Depleted,
    Quarantined
}

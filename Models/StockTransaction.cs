namespace PharmacyInventoryAPI.Models;

public class StockTransaction
{
    public int Id { get; set; }
    public int BatchId { get; set; }
    public TransactionType Type { get; set; }
    public int Quantity { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

    public Batch Batch { get; set; } = null!;
}

public enum TransactionType
{
    Received,
    Dispensed,
    Returned,
    Adjusted,
    Disposed,
    Transferred
}

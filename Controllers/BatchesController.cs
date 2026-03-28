using Microsoft.AspNetCore.Mvc;
using PharmacyInventoryAPI.DTOs;
using PharmacyInventoryAPI.Services;

namespace PharmacyInventoryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BatchesController : ControllerBase
{
    private readonly IBatchService _batchService;

    public BatchesController(IBatchService batchService)
    {
        _batchService = batchService;
    }

    /// <summary>
    /// Get all batches with optional medication and status filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BatchDto>>> GetAll(
        [FromQuery] int? medicationId,
        [FromQuery] string? status)
    {
        var batches = await _batchService.GetAllAsync(medicationId, status);
        return Ok(batches);
    }

    /// <summary>
    /// Get a batch by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BatchDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BatchDto>> GetById(int id)
    {
        var batch = await _batchService.GetByIdAsync(id);
        if (batch == null) return NotFound(new { message = $"Batch with ID {id} not found." });
        return Ok(batch);
    }

    /// <summary>
    /// Look up a batch by its batch number.
    /// </summary>
    [HttpGet("by-number/{batchNumber}")]
    [ProducesResponseType(typeof(BatchDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BatchDto>> GetByBatchNumber(string batchNumber)
    {
        var batch = await _batchService.GetByBatchNumberAsync(batchNumber);
        if (batch == null) return NotFound(new { message = $"Batch '{batchNumber}' not found." });
        return Ok(batch);
    }

    /// <summary>
    /// Create a new batch for a medication.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BatchDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BatchDto>> Create([FromBody] CreateBatchDto dto)
    {
        var batch = await _batchService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = batch.Id }, batch);
    }

    /// <summary>
    /// Update batch details (quantity, price, supplier, status).
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(BatchDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BatchDto>> Update(int id, [FromBody] UpdateBatchDto dto)
    {
        var batch = await _batchService.UpdateAsync(id, dto);
        if (batch == null) return NotFound(new { message = $"Batch with ID {id} not found." });
        return Ok(batch);
    }

    /// <summary>
    /// Delete a batch (only if depleted or empty).
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _batchService.DeleteAsync(id);
        if (!result) return NotFound(new { message = $"Batch with ID {id} not found." });
        return NoContent();
    }

    /// <summary>
    /// Record a stock transaction (dispense, receive, return, adjust, dispose, transfer).
    /// </summary>
    [HttpPost("transactions")]
    [ProducesResponseType(typeof(StockTransactionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<StockTransactionDto>> RecordTransaction(
        [FromBody] CreateStockTransactionDto dto)
    {
        var transaction = await _batchService.RecordTransactionAsync(dto);
        return CreatedAtAction(nameof(GetTransactions), new { batchId = transaction.BatchId }, transaction);
    }

    /// <summary>
    /// Get stock transaction history with optional filters.
    /// </summary>
    [HttpGet("transactions")]
    [ProducesResponseType(typeof(IEnumerable<StockTransactionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<StockTransactionDto>>> GetTransactions(
        [FromQuery] int? batchId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var transactions = await _batchService.GetTransactionsAsync(batchId, from, to);
        return Ok(transactions);
    }
}

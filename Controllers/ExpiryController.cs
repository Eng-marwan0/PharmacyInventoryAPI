using Microsoft.AspNetCore.Mvc;
using PharmacyInventoryAPI.DTOs;
using PharmacyInventoryAPI.Services;

namespace PharmacyInventoryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ExpiryController : ControllerBase
{
    private readonly IExpiryService _expiryService;

    public ExpiryController(IExpiryService expiryService)
    {
        _expiryService = expiryService;
    }

    /// <summary>
    /// Get a comprehensive expiry report with expired, critical, and warning batches.
    /// </summary>
    [HttpGet("report")]
    [ProducesResponseType(typeof(ExpiryReportDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ExpiryReportDto>> GetExpiryReport(
        [FromQuery] int warningDays = 90,
        [FromQuery] int criticalDays = 30)
    {
        var report = await _expiryService.GetExpiryReportAsync(warningDays, criticalDays);
        return Ok(report);
    }

    /// <summary>
    /// Get batches expiring within a specified number of days.
    /// </summary>
    [HttpGet("expiring")]
    [ProducesResponseType(typeof(IEnumerable<ExpiryAlertDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ExpiryAlertDto>>> GetExpiringBatches(
        [FromQuery] int days = 90)
    {
        var batches = await _expiryService.GetExpiringBatchesAsync(days);
        return Ok(batches);
    }

    /// <summary>
    /// Get all expired batches that still have stock.
    /// </summary>
    [HttpGet("expired")]
    [ProducesResponseType(typeof(IEnumerable<ExpiryAlertDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ExpiryAlertDto>>> GetExpiredBatches()
    {
        var batches = await _expiryService.GetExpiredBatchesAsync();
        return Ok(batches);
    }

    /// <summary>
    /// Scan and mark all expired batches. Returns count of newly marked batches.
    /// </summary>
    [HttpPost("mark-expired")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkExpiredBatches()
    {
        var count = await _expiryService.MarkExpiredBatchesAsync();
        return Ok(new { markedCount = count, message = $"{count} batch(es) marked as expired." });
    }

    /// <summary>
    /// Get overall inventory summary with stock, expiry, and value statistics.
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(InventorySummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<InventorySummaryDto>> GetInventorySummary()
    {
        var summary = await _expiryService.GetInventorySummaryAsync();
        return Ok(summary);
    }
}

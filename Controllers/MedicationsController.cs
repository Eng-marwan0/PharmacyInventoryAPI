using Microsoft.AspNetCore.Mvc;
using PharmacyInventoryAPI.DTOs;
using PharmacyInventoryAPI.Services;

namespace PharmacyInventoryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MedicationsController : ControllerBase
{
    private readonly IMedicationService _medicationService;

    public MedicationsController(IMedicationService medicationService)
    {
        _medicationService = medicationService;
    }

    /// <summary>
    /// Get all medications with optional search and category filter.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MedicationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MedicationDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? category)
    {
        var medications = await _medicationService.GetAllAsync(search, category);
        return Ok(medications);
    }

    /// <summary>
    /// Get a medication by ID with batch details.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(MedicationDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MedicationDetailDto>> GetById(int id)
    {
        var medication = await _medicationService.GetByIdAsync(id);
        if (medication == null) return NotFound(new { message = $"Medication with ID {id} not found." });
        return Ok(medication);
    }

    /// <summary>
    /// Create a new medication.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(MedicationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MedicationDto>> Create([FromBody] CreateMedicationDto dto)
    {
        var medication = await _medicationService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = medication.Id }, medication);
    }

    /// <summary>
    /// Update an existing medication.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(MedicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MedicationDto>> Update(int id, [FromBody] UpdateMedicationDto dto)
    {
        var medication = await _medicationService.UpdateAsync(id, dto);
        if (medication == null) return NotFound(new { message = $"Medication with ID {id} not found." });
        return Ok(medication);
    }

    /// <summary>
    /// Delete a medication (only if no active stock).
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _medicationService.DeleteAsync(id);
        if (!result) return NotFound(new { message = $"Medication with ID {id} not found." });
        return NoContent();
    }

    /// <summary>
    /// Get medications with stock below reorder level.
    /// </summary>
    [HttpGet("low-stock")]
    [ProducesResponseType(typeof(IEnumerable<MedicationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MedicationDto>>> GetLowStock()
    {
        var medications = await _medicationService.GetLowStockAsync();
        return Ok(medications);
    }
}

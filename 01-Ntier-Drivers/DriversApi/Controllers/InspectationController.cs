using Drivers.BLL.DTOs.Requests;
using Drivers.BLL.DTOs.Responses;
using Drivers.BLL.Managers.Contracts;
using Drivers.BLL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivers.Api.Controllers;

/// <summary>
/// Контролер управління техоглядами вантажівок.
/// Підтримує повний REST CRUD, фільтрацію за авто та перемикання EF Core / Dapper.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class InspectationController : ControllerBase
{
    private readonly ILogger<InspectationController> _logger;
    private readonly IInspectationManager _inspectationManager;

    public InspectationController(
        ILogger<InspectationController> logger,
        IInspectationManager inspectationManager)
    {
        _logger = logger;
        _inspectationManager = inspectationManager;
    }

    /// <summary>
    /// Отримання всіх техоглядів
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<InspectionResponceDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<InspectionResponceDTO>>> GetAll(
        [FromQuery] DataAccessProvider provider = DataAccessProvider.EntityFramework,
        CancellationToken cancellationToken = default)
    {
        var inspections = await _inspectationManager.GetAllInspectionsAsync(provider, cancellationToken);
        return Ok(inspections);
    }

    /// <summary>
    /// Отримання техогляду за ідентифікатором
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(InspectionResponceDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InspectionResponceDTO>> GetById(
        int id,
        [FromQuery] DataAccessProvider provider = DataAccessProvider.EntityFramework,
        CancellationToken cancellationToken = default)
    {
        var inspection = await _inspectationManager.GetInspectionByIdAsync(id, provider, cancellationToken);
        if (inspection == null)
        {
            return NotFound($"Техогляд з Id {id} не знайдено.");
        }
        return Ok(inspection);
    }

    /// <summary>
    /// Отримання списку техоглядів конкретної вантажівки
    /// </summary>
    [AllowAnonymous]
    [HttpGet("truck/{truckId}")]
    [ProducesResponseType(typeof(IEnumerable<InspectionResponceDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<InspectionResponceDTO>>> GetByTruck(
        int truckId,
        [FromQuery] DataAccessProvider provider = DataAccessProvider.EntityFramework,
        CancellationToken cancellationToken = default)
    {
        var inspections = await _inspectationManager.GetInspectionsByTruckIdAsync(truckId, provider, cancellationToken);
        return Ok(inspections);
    }

    /// <summary>
    /// Створення нового запису про техогляд
    /// </summary>
    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(typeof(InspectionResponceDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<InspectionResponceDTO>> Create(
        [FromBody] InspectionReqDTO dto,
        [FromQuery] DataAccessProvider provider = DataAccessProvider.EntityFramework,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var created = await _inspectationManager.CreateInspectionAsync(dto, provider, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Оновлення даних техогляду
    /// </summary>
    [AllowAnonymous]
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(InspectionResponceDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InspectionResponceDTO>> Update(
        int id,
        [FromBody] InspectionReqDTO dto,
        [FromQuery] DataAccessProvider provider = DataAccessProvider.EntityFramework,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _inspectationManager.UpdateInspectionAsync(id, dto, provider, cancellationToken);
        if (updated == null)
        {
            return NotFound($"Техогляд з Id {id} не знайдено для оновлення.");
        }
        return Ok(updated);
    }

    /// <summary>
    /// Видалення запису про техогляд
    /// </summary>
    [AllowAnonymous]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        int id,
        [FromQuery] DataAccessProvider provider = DataAccessProvider.EntityFramework,
        CancellationToken cancellationToken = default)
    {
        var deleted = await _inspectationManager.DeleteInspectionAsync(id, provider, cancellationToken);
        if (!deleted)
        {
            return NotFound($"Техогляд з Id {id} не знайдено для видалення.");
        }
        return NoContent();
    }
}

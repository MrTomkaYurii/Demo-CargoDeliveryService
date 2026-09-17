using Drivers.BLL.DTOs.Requests;
using Drivers.BLL.DTOs.Responses;
using Drivers.BLL.Managers.Contracts;
using Drivers.BLL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivers.Api.Controllers;

/// <summary>
/// Контролер управління ремонтами вантажівок.
/// Підтримує повний REST CRUD, вибірки за авто та перемикання EF Core / Dapper.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class RepairController : ControllerBase
{
    private readonly ILogger<RepairController> _logger;
    private readonly IRepairManager _repairManager;

    public RepairController(
        ILogger<RepairController> logger,
        IRepairManager repairManager)
    {
        _logger = logger;
        _repairManager = repairManager;
    }

    /// <summary>
    /// Отримання списку всіх ремонтів
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RepairResponceDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RepairResponceDTO>>> GetAll(
        [FromQuery] DataAccessProvider provider = DataAccessProvider.EntityFramework,
        CancellationToken cancellationToken = default)
    {
        var repairs = await _repairManager.GetAllRepairsAsync(provider, cancellationToken);
        return Ok(repairs);
    }

    /// <summary>
    /// Отримання інформації про ремонт за ідентифікатором
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RepairResponceDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RepairResponceDTO>> GetById(
        int id,
        [FromQuery] DataAccessProvider provider = DataAccessProvider.EntityFramework,
        CancellationToken cancellationToken = default)
    {
        var repair = await _repairManager.GetRepairByIdAsync(id, provider, cancellationToken);
        if (repair == null)
        {
            return NotFound($"Ремонт з Id {id} не знайдено.");
        }
        return Ok(repair);
    }

    /// <summary>
    /// Отримання списку ремонтів для конкретної вантажівки
    /// </summary>
    [AllowAnonymous]
    [HttpGet("truck/{truckId}")]
    [ProducesResponseType(typeof(IEnumerable<RepairResponceDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RepairResponceDTO>>> GetByTruck(
        int truckId,
        [FromQuery] DataAccessProvider provider = DataAccessProvider.EntityFramework,
        CancellationToken cancellationToken = default)
    {
        var repairs = await _repairManager.GetRepairsByTruckIdAsync(truckId, provider, cancellationToken);
        return Ok(repairs);
    }

    /// <summary>
    /// Створення нового запису про ремонт
    /// </summary>
    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(typeof(RepairResponceDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RepairResponceDTO>> Create(
        [FromBody] RepairReqDTO dto,
        [FromQuery] DataAccessProvider provider = DataAccessProvider.EntityFramework,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var created = await _repairManager.CreateRepairAsync(dto, provider, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Оновлення даних про ремонт
    /// </summary>
    [AllowAnonymous]
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(RepairResponceDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RepairResponceDTO>> Update(
        int id,
        [FromBody] RepairReqDTO dto,
        [FromQuery] DataAccessProvider provider = DataAccessProvider.EntityFramework,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _repairManager.UpdateRepairAsync(id, dto, provider, cancellationToken);
        if (updated == null)
        {
            return NotFound($"Ремонт з Id {id} не знайдено для оновлення.");
        }
        return Ok(updated);
    }

    /// <summary>
    /// Видалення запису про ремонт
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
        var deleted = await _repairManager.DeleteRepairAsync(id, provider, cancellationToken);
        if (!deleted)
        {
            return NotFound($"Ремонт з Id {id} не знайдено для видалення.");
        }
        return NoContent();
    }
}

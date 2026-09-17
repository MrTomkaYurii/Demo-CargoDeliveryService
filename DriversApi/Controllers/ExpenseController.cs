using Drivers.BLL.DTOs.Requests;
using Drivers.BLL.DTOs.Responses;
using Drivers.BLL.Managers.Contracts;
using Drivers.BLL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivers.Api.Controllers;

/// <summary>
/// Контролер управління операційними витратами (рейси, пальне, обслуговування).
/// Підтримує повний REST CRUD, вибірки за водієм або авто та перемикання EF Core / Dapper.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ExpenseController : ControllerBase
{
    private readonly ILogger<ExpenseController> _logger;
    private readonly IExpenseManager _expenseManager;

    public ExpenseController(
        ILogger<ExpenseController> logger,
        IExpenseManager expenseManager)
    {
        _logger = logger;
        _expenseManager = expenseManager;
    }

    /// <summary>
    /// Отримання списку всіх витрат
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ExpenseResponceDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ExpenseResponceDTO>>> GetAll(
        [FromQuery] DataAccessProvider provider = DataAccessProvider.EntityFramework,
        CancellationToken cancellationToken = default)
    {
        var expenses = await _expenseManager.GetAllExpensesAsync(provider, cancellationToken);
        return Ok(expenses);
    }

    /// <summary>
    /// Отримання інформації про запис витрат за ідентифікатором
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ExpenseResponceDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExpenseResponceDTO>> GetById(
        int id,
        [FromQuery] DataAccessProvider provider = DataAccessProvider.EntityFramework,
        CancellationToken cancellationToken = default)
    {
        var expense = await _expenseManager.GetExpenseByIdAsync(id, provider, cancellationToken);
        if (expense == null)
        {
            return NotFound($"Запис витрат з Id {id} не знайдено.");
        }
        return Ok(expense);
    }

    /// <summary>
    /// Отримання всіх витрат за ідентифікатором водія
    /// </summary>
    [AllowAnonymous]
    [HttpGet("driver/{driverId}")]
    [ProducesResponseType(typeof(IEnumerable<ExpenseResponceDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ExpenseResponceDTO>>> GetByDriver(
        int driverId,
        [FromQuery] DataAccessProvider provider = DataAccessProvider.EntityFramework,
        CancellationToken cancellationToken = default)
    {
        var expenses = await _expenseManager.GetExpensesByDriverIdAsync(driverId, provider, cancellationToken);
        return Ok(expenses);
    }

    /// <summary>
    /// Отримання всіх витрат за ідентифікатором вантажівки
    /// </summary>
    [AllowAnonymous]
    [HttpGet("truck/{truckId}")]
    [ProducesResponseType(typeof(IEnumerable<ExpenseResponceDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ExpenseResponceDTO>>> GetByTruck(
        int truckId,
        [FromQuery] DataAccessProvider provider = DataAccessProvider.EntityFramework,
        CancellationToken cancellationToken = default)
    {
        var expenses = await _expenseManager.GetExpensesByTruckIdAsync(truckId, provider, cancellationToken);
        return Ok(expenses);
    }

    /// <summary>
    /// Створення нового запису витрат
    /// </summary>
    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(typeof(ExpenseResponceDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ExpenseResponceDTO>> Create(
        [FromBody] ExpenseReqDTO dto,
        [FromQuery] DataAccessProvider provider = DataAccessProvider.EntityFramework,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var created = await _expenseManager.CreateExpenseAsync(dto, provider, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Оновлення запису витрат
    /// </summary>
    [AllowAnonymous]
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ExpenseResponceDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExpenseResponceDTO>> Update(
        int id,
        [FromBody] ExpenseReqDTO dto,
        [FromQuery] DataAccessProvider provider = DataAccessProvider.EntityFramework,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _expenseManager.UpdateExpenseAsync(id, dto, provider, cancellationToken);
        if (updated == null)
        {
            return NotFound($"Запис витрат з Id {id} не знайдено для оновлення.");
        }
        return Ok(updated);
    }

    /// <summary>
    /// Видалення запису витрат
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
        var deleted = await _expenseManager.DeleteExpenseAsync(id, provider, cancellationToken);
        if (!deleted)
        {
            return NotFound($"Запис витрат з Id {id} не знайдено для видалення.");
        }
        return NoContent();
    }
}

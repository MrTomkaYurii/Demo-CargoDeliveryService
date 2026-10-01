using Drivers.BLL.DTOs.Responses;
using Drivers.BLL.Managers.Contracts;
using Drivers.BLL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivers.Api.Controllers;

/// <summary>
/// Контролер управління вантажівками (автопарком).
/// Забезпечує доступ до парку транспортних засобів, аналітику технічного обслуговування
/// та бенчмаркінг швидкодії між EF Core та Dapper.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class TruckController : ControllerBase
{
    private readonly ILogger<TruckController> _logger;
    private readonly ITruckManager _truckManager;

    public TruckController(
        ILogger<TruckController> logger,
        ITruckManager truckManager)
    {
        _logger = logger;
        _truckManager = truckManager;
    }

    /// <summary>
    /// Отримання списку всіх вантажівок
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TruckResponceDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TruckResponceDTO>>> GetAllTrucks(
        [FromQuery] DataAccessProvider provider = DataAccessProvider.EntityFramework,
        CancellationToken cancellationToken = default)
    {
        var result = await _truckManager.GetAllTrucks(provider, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Отримання детальної аналітики обслуговування, витрат на ремонти та інспекцій вантажівки
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{id}/maintenance-summary")]
    [ProducesResponseType(typeof(TruckMaintenanceSummaryResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TruckMaintenanceSummaryResponseDTO>> GetMaintenanceSummary(
        int id,
        [FromQuery] DataAccessProvider provider = DataAccessProvider.EntityFramework,
        CancellationToken cancellationToken = default)
    {
        var result = await _truckManager.GetTruckMaintenanceSummaryAsync(id, provider, cancellationToken);
        if (result == null)
        {
            return NotFound($"Вантажівку з Id {id} не знайдено.");
        }
        return Ok(result);
    }

    /// <summary>
    /// Бенчмаркінг: порівняння часу виконання вибірки аналітики вантажівки між EF Core та Dapper
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{id}/compare-maintenance")]
    [ProducesResponseType(typeof(QueryComparisonResultDTO<TruckMaintenanceSummaryResponseDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<QueryComparisonResultDTO<TruckMaintenanceSummaryResponseDTO>>> CompareMaintenance(
        int id,
        CancellationToken cancellationToken = default)
    {
        var comparison = await _truckManager.CompareTruckMaintenancePerformanceAsync(id, cancellationToken);
        return Ok(comparison);
    }
}

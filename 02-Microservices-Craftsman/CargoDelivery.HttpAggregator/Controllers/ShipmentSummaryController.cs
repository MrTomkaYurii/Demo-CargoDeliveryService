namespace CargoDelivery.HttpAggregator.Controllers;

using CargoDelivery.HttpAggregator.Domain.Dtos;
using CargoDelivery.HttpAggregator.Domain.Services;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// ====================================================================================
/// 🌐 GATEWAY AGGREGATOR CONTROLLER: ЗВЕДЕНИЙ ДАШБОРД РЕЙСУ ДОСТАВКИ
/// ------------------------------------------------------------------------------------
/// ПРИЗНАЧЕННЯ:
/// Надає єдиний вхідний REST API ендпоінт для диспетчерів та моніторингових систем,
/// що агрегує дані з трьох різних мікросервісів системи CargoDelivery.
/// ====================================================================================
/// </summary>
[ApiController]
[Route("api/v1/shipment-summary")]
public sealed class ShipmentSummaryController : ControllerBase
{
    private readonly IShipmentAggregatorService _aggregatorService;
    private readonly ILogger<ShipmentSummaryController> _logger;

    public ShipmentSummaryController(
        IShipmentAggregatorService aggregatorService,
        ILogger<ShipmentSummaryController> logger)
    {
        _aggregatorService = aggregatorService;
        _logger = logger;
    }

    /// <summary>
    /// Отримує зведену інформацію про рейс водія, активні доставки та дорожні замітки.
    /// </summary>
    /// <param name="driverId">Ідентифікатор водія (Guid).</param>
    /// <param name="cancellationToken">Токен скасування запиту.</param>
    /// <returns>Композитний DTO ShipmentSummaryDto.</returns>
    [HttpGet("{driverId:guid}")]
    [ProducesResponseType(typeof(ShipmentSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShipmentSummaryDto>> GetSummary(
        [FromRoute] Guid driverId,
        CancellationToken cancellationToken)
    {
        try
        {
            var summary = await _aggregatorService.GetShipmentSummaryAsync(driverId, cancellationToken);
            return Ok(summary);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex.Message);
            return NotFound(new { message = ex.Message });
        }
    }
}

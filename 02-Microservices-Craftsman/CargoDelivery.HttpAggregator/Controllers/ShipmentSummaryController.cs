namespace CargoDelivery.HttpAggregator.Controllers;

using CargoDelivery.HttpAggregator.Domain.Dtos;
using CargoDelivery.HttpAggregator.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Polly.CircuitBreaker;
using Polly.Timeout;

/// <summary>
/// ====================================================================================
/// 🌐 GATEWAY AGGREGATOR CONTROLLER: ЗВЕДЕНИЙ ДАШБОРД РЕЙСУ ДОСТАВКИ
/// ------------------------------------------------------------------------------------
/// ПРИЗНАЧЕННЯ:
/// Надає єдиний вхідний REST API ендпоінт для диспетчерів та моніторингових систем,
/// що агрегує дані з трьох різних мікросервісів системи CargoDelivery.
///
/// КАРТА HTTP-ВІДПОВІДЕЙ:
/// - 200 OK                  — повне або частково деградоване (IsDegraded = true) зведення;
/// - 404 Not Found           — водія не існує (бізнес-результат, а не збій);
/// - 503 Service Unavailable — недоступна ЖОРСТКА залежність (водії або доставки)
///                             навіть після всіх повторів Polly.
/// Контролер тонкий: уся логіка агрегації живе в ShipmentAggregatorService.
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
    /// <param name="cancellationToken">Токен скасування запиту (спрацьовує, якщо клієнт закрив з'єднання).</param>
    /// <returns>Композитний DTO ShipmentSummaryDto.</returns>
    [HttpGet("{driverId:guid}")]
    [ProducesResponseType(typeof(ShipmentSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
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
            _logger.LogWarning("{Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex) when (ex is HttpRequestException or TimeoutRejectedException or BrokenCircuitException)
        {
            // Жорстка залежність недоступна: замість "сирого" 500 зі стектрейсом повертаємо
            // стандартизований ProblemDetails (RFC 9457) зі статусом 503 — клієнт розуміє,
            // що варто повторити пізніше, а моніторинг бачить чітку причину.
            _logger.LogError(ex, "Критична залежність недоступна під час агрегації для водія {DriverId}", driverId);
            return Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Критичний мікросервіс тимчасово недоступний",
                detail: $"{ex.GetType().Name}: {ex.Message}");
        }
    }
}

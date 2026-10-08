namespace CargoDelivery.HttpAggregator.Domain.Services;

using CargoDelivery.HttpAggregator.Domain.Clients;
using CargoDelivery.HttpAggregator.Domain.Dtos;
using Microsoft.Extensions.Logging;

/// <summary>
/// ====================================================================================
/// 🏛️ GATEWAY AGGREGATOR SERVICE: ПАРАЛЕЛЬНИЙ FAN-OUT / FAN-IN ТА ДЕГРАДАЦІЯ
/// ------------------------------------------------------------------------------------
/// АРХІТЕКТУРНИЙ ДИЗАЙН:
/// 1. Scatter-Gather Pattern:
///    Замість послідовних викликів, запити до мікросервісів замовлень та блогу
///    виконуються паралельно за допомогою Task.WhenAll.
///
/// 2. Graceful Degradation (Часткова деградація):
///    Якщо допоміжний мікросервіс (DriversBlogManagement) недоступний або відповідає
///    помилкою 503, загальний запит агрегатора НЕ падає з кодом 500.
///    Дані водія та замовлень повертаються успішно, прапорець IsDegraded встановлюється в true,
///    а стан фіксується у логах та трасуванні OpenTelemetry.
/// ====================================================================================
/// </summary>
public sealed class ShipmentAggregatorService : IShipmentAggregatorService
{
    private readonly IDriverApiClient _driverApiClient;
    private readonly ICargoOrderApiClient _cargoOrderApiClient;
    private readonly IDriverBlogApiClient _driverBlogApiClient;
    private readonly ILogger<ShipmentAggregatorService> _logger;

    public ShipmentAggregatorService(
        IDriverApiClient driverApiClient,
        ICargoOrderApiClient cargoOrderApiClient,
        IDriverBlogApiClient driverBlogApiClient,
        ILogger<ShipmentAggregatorService> logger)
    {
        _driverApiClient = driverApiClient;
        _cargoOrderApiClient = cargoOrderApiClient;
        _driverBlogApiClient = driverBlogApiClient;
        _logger = logger;
    }

    public async Task<ShipmentSummaryDto> GetShipmentSummaryAsync(Guid driverId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Початок агрегації рейсу для водія {DriverId}", driverId);

        // КРОК 1: Перевірка водія у сервісі DriversManagement
        var driver = await _driverApiClient.GetDriverByIdAsync(driverId, cancellationToken);
        if (driver == null)
        {
            _logger.LogWarning("Водія {DriverId} не знайдено при агрегації. Повертаємо 404.", driverId);
            throw new KeyNotFoundException($"Водія з ID {driverId} не знайдено в системі DriversManagement.");
        }

        bool isDegraded = false;

        // КРОК 2: Паралельний Fan-Out до CargoOrderingService та DriversBlogManagement
        var ordersTask = _cargoOrderApiClient.GetActiveOrdersByDriverIdAsync(driverId, cancellationToken);

        var blogNotesTask = SafeFetchRoadNotesAsync(
            () => _driverBlogApiClient.GetRoadNotesByDriverIdAsync(driverId, cancellationToken),
            onFailure: () => isDegraded = true);

        // КРОК 3: Fan-In через Task.WhenAll
        await Task.WhenAll(ordersTask, blogNotesTask);

        var orders = await ordersTask;
        var notes = await blogNotesTask;

        // КРОК 4: Формування підсумкового DTO
        var summary = new ShipmentSummaryDto
        {
            DriverId = driverId,
            Driver = driver,
            ActiveOrders = orders,
            RoadNotes = notes,
            IsDegraded = isDegraded,
            AggregationStatus = isDegraded ? "Partial (Degraded)" : "Complete",
            AggregatedAtUtc = DateTime.UtcNow
        };

        _logger.LogInformation("Агрегацію рейсу для водія {DriverId} завершено. Статус: {Status}", 
            driverId, summary.AggregationStatus);

        return summary;
    }

    private async Task<List<AggregatedBlogNoteDto>> SafeFetchRoadNotesAsync(
        Func<Task<List<AggregatedBlogNoteDto>>> fetchAction,
        Action onFailure)
    {
        try
        {
            return await fetchAction();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Мікросервіс DriversBlogManagement недоступний. Застосовано Graceful Degradation.");
            onFailure();
            return [];
        }
    }
}

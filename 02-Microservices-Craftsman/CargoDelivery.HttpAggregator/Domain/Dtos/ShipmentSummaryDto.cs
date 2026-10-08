namespace CargoDelivery.HttpAggregator.Domain.Dtos;

/// <summary>
/// ====================================================================================
/// 📦 SHIPMENT SUMMARY DTO (КОМПОЗИТНИЙ РЕЗУЛЬТАТ АГРЕГАЦІЇ)
/// ------------------------------------------------------------------------------------
/// ПРИЗНАЧЕННЯ:
/// Єдиний зведений об'єкт диспетчера, сформований у результаті виконання патерну
/// Gateway Aggregator (Scatter-Gather) шляхом паралельного опитування трьох мікросервісів:
/// 1. DriversManagement -> Driver
/// 2. CargoOrderingService -> ActiveOrders
/// 3. DriversBlogManagement -> RoadNotes
///
/// GRACEFUL DEGRADATION:
/// Поле IsDegraded сигналізує клієнту, чи всі дані були отримані, чи один із сервісів
/// (наприклад, блог) був недоступний і відповідь сформована частково.
/// ====================================================================================
/// </summary>
public sealed class ShipmentSummaryDto
{
    public Guid DriverId { get; set; }
    public AggregatedDriverDto Driver { get; set; } = new();
    public List<AggregatedCargoOrderDto> ActiveOrders { get; set; } = [];
    public List<AggregatedBlogNoteDto> RoadNotes { get; set; } = [];
    public bool IsDegraded { get; set; }
    public string AggregationStatus { get; set; } = "Complete";
    public DateTime AggregatedAtUtc { get; set; } = DateTime.UtcNow;
}

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

        // КРОК 1: Перевірка водія у сервісі DriversManagement (ПОСЛІДОВНИЙ бар'єр, Guard Check).
        // Водій — корінь агрегації. Якщо його не існує, опитувати замовлення та блог немає сенсу:
        // ми економимо 2 мережеві виклики і не створюємо зайвого навантаження на сусідні сервіси.
        var driver = await _driverApiClient.GetDriverByIdAsync(driverId, cancellationToken);
        if (driver == null)
        {
            _logger.LogWarning("Водія {DriverId} не знайдено при агрегації. Повертаємо 404.", driverId);
            throw new KeyNotFoundException($"Водія з ID {driverId} не знайдено в системі DriversManagement.");
        }

        // Прапорець часткової деградації. Його змінює лямбда onFailure у SafeFetchRoadNotesAsync.
        bool isDegraded = false;

        // КРОК 2: Паралельний Fan-Out (Scatter) до CargoOrderingService та DriversBlogManagement.
        // Зверніть увагу: тут НЕМАЄ await — ми лише ЗАПУСКАЄМО обидва HTTP-запити. Обидва вже
        // летять мережею одночасно, а потік не блокується (очікування I/O не займає потоків пулу).
        // Доставки — жорстка залежність: їхній виняток НЕ перехоплюється і дійде до контролера (503).
        var ordersTask = _cargoOrderApiClient.GetActiveOrdersByDriverIdAsync(driverId, cancellationToken);

        var blogNotesTask = SafeFetchRoadNotesAsync(
            () => _driverBlogApiClient.GetRoadNotesByDriverIdAsync(driverId, cancellationToken),
            onFailure: () => isDegraded = true);

        // КРОК 3: Fan-In (Gather) через Task.WhenAll — чекаємо завершення ОБОХ задач.
        // Загальний час ≈ max(t_orders, t_blog), а не їхня сума.
        // Якщо ordersTask впаде, await Task.WhenAll перекине ПЕРШИЙ виняток з AggregateException.
        await Task.WhenAll(ordersTask, blogNotesTask);

        // Задачі вже завершені, тож ці await миттєво повертають результат (без повторного виклику)
        var orders = await ordersTask;
        var notes = await blogNotesTask;

        // КРОК 4: Композиція (Composition) — формування підсумкового DTO під потреби диспетчера
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

                        // якщо потрібно 300мс на весь запит від агрегатора
                        // Створюємо таймаут на 300 мілісекунд
                        //using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                        //cts.CancelAfter(TimeSpan.FromMilliseconds(300));

                        //try
                        //{
                        //    // Запускаємо паралельно
                        //    var ordersTask = _cargoOrderApiClient.GetActiveOrdersByDriverIdAsync(driverId, cts.Token);

                        //var blogNotesTask = SafeFetchRoadNotesAsync(
                        //    () => _driverBlogApiClient.GetRoadNotesByDriverIdAsync(driverId, cts.Token),
                        //    onFailure: () => isDegraded = true);

                        //// Чекаємо із загальним лімітом часу
                        //await Task.WhenAll(ordersTask, blogNotesTask);
                        //}
                        //v catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                        //{
                        //    // Спрацював саме наш таймаут 300 мс (а не скасування від клієнта)
                        //    _logger.LogWarning("Перевищено ліміт часу агрегації (300 мс). Застосовано аварійну деградацію.");
                        //isDegraded = true;
    
                        //    // Якщо замовлення критичні і вони теж не встигли — це фатал, прокидаємо помилку.
                        //    // Якщо замовлення вже встигли завершитися, а блог завис — можемо взяти те, що є.
                        //}


    /// <summary>
    /// Обгортка відмовостійкості (Fault Isolation) для НЕКРИТИЧНОЇ залежності — блогу.
    /// Будь-який збій (HTTP 5xx після повторів Polly, таймаут, розімкнений Circuit Breaker)
    /// перетворюється на порожній список + прапорець деградації, а не на HTTP 500 для клієнта.
    /// Скасування запиту (OperationCanceledException) свідомо НЕ ковтається: якщо клієнт
    /// пішов, продовжувати агрегацію немає сенсу.
    /// </summary>
    private async Task<List<AggregatedBlogNoteDto>> SafeFetchRoadNotesAsync(
        Func<Task<List<AggregatedBlogNoteDto>>> fetchAction,
        Action onFailure)
    {
        try
        {
            return await fetchAction();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Warning, а не Error: збій передбачений і опрацьований — сервіс продовжує працювати
            _logger.LogWarning(ex, "Мікросервіс DriversBlogManagement недоступний. Застосовано Graceful Degradation.");
            onFailure();
            return [];
        }
    }
}

namespace DriversManagement.Domain.Drivers.Features;

using DriversManagement.Domain.Drivers.Dtos;
using DriversManagement.Domain.Drivers.Services;
using DriversManagement.Domain.Drivers.Services.Http;
using DriversManagement.Domain.Trucks.Services;
using DriversManagement.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// ====================================================================================
/// 🏛️ CQRS FEATURE: GET DRIVER DASHBOARD (GATEWAY AGGREGATOR / BFF HANDLER)
/// ------------------------------------------------------------------------------------
/// АРХІТЕКТУРНИЙ ДИЗАЙН ТА ПРИНЦИП РОБОТИ:
/// 
/// 1. ПРИЗНАЧЕННЯ:
///    Реалізація патерну Composite API / Gateway Aggregator (або Backend for Frontend, BFF).
///    Замість того, щоб клієнтський додаток (веб-портал, мобільний застосунок або WPF)
///    виконував 3 окремих мережевих виклики через інтернет:
///      - GET /api/v1/drivers/{id} (до DriversManagement)
///      - GET /api/v1/deliveries (до CargoOrderingService)
///      - GET /api/v1/postaboutdrivers (до DriversBlogManagement)
///    Клієнт робить ОДИН легкий запит:
///      -> GET /api/v1/driver-dashboard/{id}
/// 
/// 2. ЧОМУ САМЕ "Task.WhenAll", А НЕ ПОСЛІДОВНІ "await"?
///    - Послідовні виклики (await A; await B; await C;) сумують час очікування мережі:
///      Загальний час = Latency(A) + Latency(B) + Latency(C) = 45ms + 60ms + 40ms = 145ms.
///    - Паралельні виклики через Task.WhenAll відправляють запити в сокети ОДНОЧАСНО:
///      Загальний час = Max(Latency(A), Latency(B), Latency(C)) = Max(45, 60, 40) = 60ms!
///      Це зменшує час відгуку для кінцевого користувача більш ніж у 2.4 раза.
///
/// 3. ПАТЕРН СТІЙКОСТІ "GRACEFUL DEGRADATION" (ЧАСТКОВА ДЕГРАДАЦІЯ):
///    Якщо блог або система замовлень тимчасово недоступні (503 Service Unavailable,
///    таймаут, розімкнений Circuit Breaker у Polly), ми НЕ «кладемо» весь дашборд з кодом 500!
///    Метод SafeFetchAsync перехоплює мережевий збій, фіксує його в логах (OpenTelemetry)
///    і повертає дефолтне значення (порожній список).
///    Клієнт отримує статус 200 OK з даними водія і прапорцем IsPartialData = true.
/// ====================================================================================
/// </summary>
public static class GetDriverDashboard
{
    /// <summary>
    /// Незмінний CQRS-запит (Query) для отримання агрегованої панелі водія.
    /// </summary>
    public sealed record Query(Guid DriverId) : IRequest<AggregatedDriverDashboardDto>;

    /// <summary>
    /// Обробник запиту MediatR з інжекцією локальних репозиторіїв та типізованих HTTP-клієнтів.
    /// </summary>
    public sealed class Handler : IRequestHandler<Query, AggregatedDriverDashboardDto>
    {
        private readonly IDriverRepository _driverRepository;
        private readonly ITruckRepository _truckRepository;
        private readonly ICargoOrderingApiClient _orderingApiClient;
        private readonly IDriversBlogApiClient _blogApiClient;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IDriverRepository driverRepository,
            ITruckRepository truckRepository,
            ICargoOrderingApiClient orderingApiClient,
            IDriversBlogApiClient blogApiClient,
            ILogger<Handler> logger)
        {
            _driverRepository = driverRepository;
            _truckRepository = truckRepository;
            _orderingApiClient = orderingApiClient;
            _blogApiClient = blogApiClient;
            _logger = logger;
        }

        public async Task<AggregatedDriverDashboardDto> Handle(Query request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Початок агрегації даних дашборду для водія {DriverId}", request.DriverId);

            // КРОК 1: Перевірка існування базової сутності водія у локальній базі даних.
            // Якщо водія не існує — негайно повертаємо NotFoundException і не витрачаємо ресурси
            // на зовнішні HTTP-запити до сторонніх сервісів.
            var driver = await _driverRepository.GetById(request.DriverId, cancellationToken: cancellationToken);
            if (driver == null)
            {
                throw new NotFoundException($"Водія з ідентифікатором {request.DriverId} не знайдено в DriversManagement.");
            }

            bool hadPartialFailures = false;

            // КРОК 2: Ініціалізація паралельного Fan-Out (запуск завдань без негайного блокування)
            // Виклики стартують одночасно в пулі потоків та очікують завершення вводу-виводу (I/O).
            var orderingTask = SafeFetchAsync(
                action: () => _orderingApiClient.GetActiveDeliveriesByDriverAsync(request.DriverId, cancellationToken),
                defaultValue: new List<AggregatedDeliveryDto>(),
                onFailure: () => hadPartialFailures = true);

            var blogTask = SafeFetchAsync(
                action: () => _blogApiClient.GetDriverPostsAsync(request.DriverId, cancellationToken),
                defaultValue: new List<AggregatedBlogPostDto>(),
                onFailure: () => hadPartialFailures = true);

            // Локальний запит до бази даних для отримання закріпленої вантажівки (також паралельно)
            var truckTask = _truckRepository.Query().AsNoTracking().FirstOrDefaultAsync(cancellationToken);

            // КРОК 3: Fan-In через Task.WhenAll.
            // Очікуємо одночасного завершення зовнішніх HTTP REST викликів та локального EF Core запиту.
            await Task.WhenAll(orderingTask, blogTask, truckTask);

            var deliveries = await orderingTask;
            var blogPosts = await blogTask;
            var truck = await truckTask;

            // КРОК 4: Компонування єдиного зведеного об'єкта AggregatedDriverDashboardDto
            var result = new AggregatedDriverDashboardDto
            {
                DriverId = driver.Id,
                FullName = $"{driver.FirstName} {driver.LastName}".Trim(),
                LicenseNumber = driver.DriverLicenseNumber,
                CompanyName = driver.Company?.CompanyName ?? "Приватний перевізник",
                AssignedTruckInfo = truck != null ? $"{truck.Model} (Держномер: {truck.TruckNumber})" : "Автомобіль не призначено",
                ActiveDeliveries = deliveries,
                RecentBlogPosts = blogPosts,
                IsPartialData = hadPartialFailures
            };

            _logger.LogInformation("Формування дашборду водія {DriverId} успішно завершено. Часткові збої: {HadFailures}",
                request.DriverId, hadPartialFailures);

            return result;
        }

        /// <summary>
        /// Допоміжний механізм захисту від відмов (Fault Tolerance Wrapper):
        /// Запобігає повному аварійному завершенню агрегатора при збоях окремих сервісів.
        /// </summary>
        private async Task<T> SafeFetchAsync<T>(Func<Task<T>> action, T defaultValue, Action onFailure)
        {
            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, 
                    "Виклик зовнішнього мікросервісу зазнав збою. Застосовано патерн Graceful Degradation.");
                onFailure();
                return defaultValue;
            }
        }
    }
}

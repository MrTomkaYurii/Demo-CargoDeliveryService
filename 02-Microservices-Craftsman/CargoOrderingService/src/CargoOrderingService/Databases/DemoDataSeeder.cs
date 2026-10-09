namespace CargoOrderingService.Databases;

using Bogus;
using CargoOrderingService.Domain.Deliveries.Models;
using CargoOrderingService.Domain.Orders;
using CargoOrderingService.Domain.Orders.Models;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// ====================================================================================
/// 🌱 ДЕМО-СІДИНГ CargoOrderingService (Bogus) — ЗАМОВЛЕННЯ ТА ДОСТАВКИ
/// ------------------------------------------------------------------------------------
/// ПРИЗНАЧЕННЯ:
/// Створює 50 замовлень (Order) разом із доставками (Delivery), призначеними тіньовим
/// водіям, щоб BFF (driver-dashboard) і агрегатор (shipment-summary) показували рейси.
///
/// ЧОМУ СПЕРШУ ЧЕКАЄМО:
/// Тіньових водіїв цей сервіс отримує асинхронно — подією IDriverCreated від
/// DriversManagement (споживач SyncDriverInOrdering). Сідер чекає, доки кількість водіїв
/// стабілізується, і лише тоді призначає їм замовлення (Eventual Consistency).
///
/// МОДЕЛЬ:
/// Order ↔ Delivery — "один-до-одного" (Order отримує Id своєї Delivery). Order вже має
/// порожню Delivery за замовчуванням, тому ми заповнюємо її доменним методом Update(...)
/// і призначаємо водія і замовленню, і доставці (SetDriver).
///
/// Працює лише в Development і лише якщо замовлень ще немає.
/// ====================================================================================
/// </summary>
public sealed class DemoDataSeeder : BackgroundService
{
    private const int OrdersCount = 50;
    private const int FakerSeed = 2026;

    private static readonly string[] Cities = ["Київ", "Львів", "Харків", "Одеса", "Дніпро", "Вінниця", "Полтава", "Запоріжжя", "Чернівці", "Ужгород", "Луцьк", "Суми"];
    private static readonly string[] Cargo =
        ["Будівельні матеріали", "Медичне обладнання", "Продукти харчування (рефрижератор)", "Побутова техніка", "Зерно", "Металопрокат", "Меблі", "Автозапчастини"];
    private static readonly string[] Customers =
        ["ТОВ «Епіцентр-Логістик»", "ПрАТ «Агро-Схід»", "ТОВ «Фармак-Дистрибуція»", "ФОП Коваленко", "ТОВ «Метал-Груп»", "ТОВ «Меблі Карпат»"];

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DemoDataSeeder> _logger;

    public DemoDataSeeder(IServiceScopeFactory scopeFactory, ILogger<DemoDataSeeder> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<OrderingContext>();

            if (await db.Orders.AnyAsync(stoppingToken))
            {
                _logger.LogInformation("🌱 Сідинг пропущено: замовлення вже існують.");
                return;
            }

            await WaitForReplicatedDriversAsync(db, stoppingToken);
            var drivers = await db.Drivers.OrderBy(d => d.Id).ToListAsync(stoppingToken);
            if (drivers.Count == 0)
            {
                _logger.LogWarning("🌱 Тіньові водії не надійшли за відведений час — сідинг замовлень пропущено.");
                return;
            }

            var faker = new Faker("uk") { Random = new Randomizer(FakerSeed) };
            for (var i = 1; i <= OrdersCount; i++)
            {
                // Кожен водій отримує щонайменше одне замовлення (по колу)
                var driver = drivers[(i - 1) % drivers.Count];
                var deliveryDate = faker.Date.Soon(20).ToString("yyyy-MM-dd");
                var from = faker.PickRandom(Cities);
                var to = faker.PickRandom(Cities.Where(c => c != from).ToArray());

                var order = Order.Create(new OrderForCreation
                {
                    OrderNumber = $"ORD-2026-{i:0000}",
                    CustomerName = faker.PickRandom(Customers),
                    DeliveryDate = deliveryDate,
                    TotalAmount = $"{faker.Random.Int(8_000, 120_000)} UAH",
                    Status = faker.PickRandom("New", "Confirmed", "InProgress", "Completed")
                }).SetDriver(driver);

                order.Delivery.Update(new DeliveryForUpdate
                {
                    DeliveryDate = deliveryDate,
                    PickupAddress = $"{from}, {faker.Address.StreetAddress()}",
                    DestinationAddress = $"{to}, {faker.Address.StreetAddress()}",
                    PackageDetails = $"{faker.PickRandom(Cargo)}, {faker.Random.Int(2, 33)} палет",
                    DeliveryStatus = faker.PickRandom("Scheduled", "InTransit", "InTransit", "Delivered")
                }).SetDriver(driver);

                await db.Orders.AddAsync(order, stoppingToken);
            }

            await db.SaveChangesAsync(stoppingToken);
            _logger.LogInformation("🌱 Створено {Count} замовлень із доставками для {Drivers} тіньових водіїв.", OrdersCount, drivers.Count);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "🌱 Помилка демо-сідингу CargoOrderingService.");
        }
    }

    /// <summary>
    /// Очікує, доки тіньові водії надійдуть через RabbitMQ: кількість > 0 і не змінюється
    /// протягом двох перевірок поспіль. Максимум ~3 хв.
    /// </summary>
    private static async Task WaitForReplicatedDriversAsync(OrderingContext db, CancellationToken stoppingToken)
    {
        var previous = -1;
        var stableChecks = 0;
        for (var attempt = 0; attempt < 90; attempt++)
        {
            var count = await db.Drivers.CountAsync(stoppingToken);
            if (count > 0 && count == previous && ++stableChecks >= 2)
                return;
            if (count != previous)
                stableChecks = 0;
            previous = count;
            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }
}

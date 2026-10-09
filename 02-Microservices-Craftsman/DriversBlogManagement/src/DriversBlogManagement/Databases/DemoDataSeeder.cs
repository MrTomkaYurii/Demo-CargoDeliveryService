namespace DriversBlogManagement.Databases;

using Bogus;
using DriversBlogManagement.Domain.PostAboutDrivers;
using DriversBlogManagement.Domain.PostAboutDrivers.Models;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// ====================================================================================
/// 🌱 ДЕМО-СІДИНГ DriversBlogManagement (Bogus) — ДОРОЖНІ ЗАМІТКИ ВОДІЇВ
/// ------------------------------------------------------------------------------------
/// ПРИЗНАЧЕННЯ:
/// Створює по одному посту ("дорожній замітці") для кожного тіньового водія, щоб gRPC
/// (GetDriverWithPosts), BFF і агрегатор повертали реальні дані.
///
/// ЧОМУ СПЕРШУ ЧЕКАЄМО:
/// Водіїв блог НЕ створює сам — їх надсилає DriversManagement подією IDriverCreated
/// через RabbitMQ (споживач SyncDriver). Отже, на момент старту таблиця drivers порожня,
/// і сідер очікує, доки реплікація завершиться (кількість водіїв перестане зростати).
/// Це і є Eventual Consistency в дії: дані з'являються "трохи згодом".
///
/// ОБМЕЖЕННЯ МОДЕЛІ:
/// У моделі Craftsman зв'язок Driver ↔ PostAboutDriver — "один-до-одного", тому
/// постів не більше, ніж водіїв (по одному на водія).
///
/// Працює лише в Development і лише якщо постів ще немає.
/// ====================================================================================
/// </summary>
public sealed class DemoDataSeeder : BackgroundService
{
    private const int FakerSeed = 2026;

    private static readonly string[] Roads = ["М-05 Київ–Одеса", "М-06 Київ–Чоп", "М-03 Київ–Харків", "М-07 Київ–Ковель", "М-10 Львів–Краковець", "Н-31 Дніпро–Решетилівка"];
    private static readonly string[] Topics =
    [
        "ремонт мосту, рух однією смугою",
        "черга на митному переході",
        "ожеледиця вранці, обережно на спусках",
        "нова зручна стоянка для фур з душем",
        "вагова перевірка на в'їзді в місто",
        "об'їзд через ремонт дорожнього полотна",
        "сильний боковий вітер на відкритій ділянці"
    ];

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
            var db = scope.ServiceProvider.GetRequiredService<BlogManagementContext>();

            if (await db.PostAboutDrivers.AnyAsync(stoppingToken))
            {
                _logger.LogInformation("🌱 Сідинг пропущено: у блозі вже є пости.");
                return;
            }

            var driverIds = await WaitForReplicatedDriversAsync(db, stoppingToken);
            if (driverIds.Count == 0)
            {
                _logger.LogWarning("🌱 Тіньові водії не надійшли за відведений час — сідинг постів пропущено.");
                return;
            }

            var faker = new Faker("uk") { Random = new Randomizer(FakerSeed) };
            foreach (var driverId in driverIds)
            {
                var road = faker.PickRandom(Roads);
                var topic = faker.PickRandom(Topics);
                var post = PostAboutDriver.Create(new PostAboutDriverForCreation
                {
                    Title = $"Траса {road}: {topic}",
                    Content = $"{faker.Date.Recent(14):dd.MM} поблизу {faker.Address.City()} — {topic}. " +
                              $"Затримка близько {faker.Random.Int(5, 90)} хв. {faker.Lorem.Sentence(8)}"
                });
                await db.PostAboutDrivers.AddAsync(post, stoppingToken);

                // Тіньовий зовнішній ключ DriverId (див. DriverConfiguration блогу)
                db.Entry(post).Property("DriverId").CurrentValue = driverId;
            }

            await db.SaveChangesAsync(stoppingToken);
            _logger.LogInformation("🌱 Створено {Count} дорожніх заміток для тіньових водіїв.", driverIds.Count);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "🌱 Помилка демо-сідингу DriversBlogManagement.");
        }
    }

    /// <summary>
    /// Очікує, доки тіньові водії надійдуть через RabbitMQ: кількість має бути > 0 і не
    /// змінюватись протягом двох перевірок поспіль (реплікацію завершено). Максимум ~3 хв.
    /// </summary>
    private async Task<List<Guid>> WaitForReplicatedDriversAsync(BlogManagementContext db, CancellationToken stoppingToken)
    {
        var previous = -1;
        var stableChecks = 0;
        for (var attempt = 0; attempt < 90; attempt++)
        {
            var count = await db.Drivers.CountAsync(stoppingToken);
            if (count > 0 && count == previous && ++stableChecks >= 2)
                break;
            if (count != previous)
                stableChecks = 0;
            previous = count;
            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }

        return await db.Drivers.AsNoTracking().OrderBy(d => d.Id).Select(d => d.Id).ToListAsync(stoppingToken);
    }
}

namespace DriversManagement.Databases;

using Bogus;
using DriversManagement.Domain.Companies;
using DriversManagement.Domain.Companies.Models;
using DriversManagement.Domain.Drivers;
using DriversManagement.Domain.Drivers.Models;
using DriversManagement.Domain.Trucks;
using DriversManagement.Domain.Trucks.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Messages;

/// <summary>
/// ====================================================================================
/// 🌱 ДЕМО-СІДИНГ DriversManagement (Bogus) — ДЖЕРЕЛО ПРАВДИ ПРО ВОДІЇВ
/// ------------------------------------------------------------------------------------
/// ПРИЗНАЧЕННЯ:
/// Контейнери Aspire ефемерні (без томів даних), тож при кожному запуску бази порожні.
/// Цей фоновий сервіс наповнює БД навчальними даними, щоб дашборд, BFF, gRPC і
/// агрегатор одразу мали що показати:
///   - 50 водіїв (українська локаль Bogus);
///   - компанії-перевізники для частини водіїв (зв'язок Company ↔ Driver "один-до-одного");
///   - 15 вантажівок автопарку.
///
/// ЧОМУ ПОДІЇ, А НЕ СІДИНГ У КОЖНОМУ СЕРВІСІ ОКРЕМО?
/// DriversManagement — єдине джерело правди про водіїв. Для кожного створеного водія
/// публікується IDriverCreated → RabbitMQ (Fanout) → тіньові копії з'являються в
/// DriversBlogManagement та CargoOrderingService ТИМИ САМИМИ Id. Тобто сідинг сам є
/// живою демонстрацією Event-Driven реплікації (видно в Aspire Dashboard → Traces).
///
/// БЕЗПЕКА ПОВТОРНОГО ЗАПУСКУ:
/// - працює лише в Development (реєстрація в WebAppServiceConfiguration);
/// - якщо водії вже є (наприклад, підключено том даних), сідинг пропускається.
///
/// ПОРЯДОК СТАРТУ:
/// MigrationHostedService зареєстрований раніше, тож до виклику ExecuteAsync схема вже
/// створена. Перед публікацією чекаємо, поки шина MassTransit стане Healthy.
/// ====================================================================================
/// </summary>
public sealed class DemoDataSeeder : BackgroundService
{
    // Скільки водіїв згенерувати і фіксоване зерно Bogus — дані однакові від запуску до запуску
    private const int DriversCount = 50;
    private const int TrucksCount = 15;
    private const int FakerSeed = 2026;

    private static readonly string[] Patronymics =
        ["Іванович", "Петрович", "Олександрович", "Васильович", "Миколайович", "Андрійович", "Сергійович", "Володимирович"];
    private static readonly string[] Cities =
        ["Київ", "Львів", "Харків", "Одеса", "Дніпро", "Вінниця", "Полтава", "Житомир", "Чернігів", "Ужгород", "Рівне", "Луцьк"];
    private static readonly string[] Carriers =
        ["Нова Логістика", "ТрансКарго", "Експрес Логістик", "Дніпро Транс", "Карпати Фрахт", "Укрвантаж", "Південний Шлях"];
    private static readonly string[] TruckModels =
        ["Volvo FH16", "Scania R500", "MAN TGX 18.510", "DAF XF 480", "Mercedes-Benz Actros", "Iveco S-Way", "Renault T High"];

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IBusControl _bus;
    private readonly ILogger<DemoDataSeeder> _logger;

    public DemoDataSeeder(IServiceScopeFactory scopeFactory, IBusControl bus, ILogger<DemoDataSeeder> logger)
    {
        _scopeFactory = scopeFactory;
        _bus = bus;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<DriversManagementContext>();

            if (await db.Drivers.AnyAsync(stoppingToken))
            {
                _logger.LogInformation("🌱 Сідинг пропущено: у БД уже є водії.");
                return;
            }

            // ---------------- 1. Водії ----------------
            var driverFaker = new Faker<DriverForCreation>("uk")
                .UseSeed(FakerSeed)
                .RuleFor(d => d.Gender, f => f.PickRandom("Male", "Female"))
                .RuleFor(d => d.FirstName, (f, d) => f.Name.FirstName(d.Gender == "Male" ? Bogus.DataSets.Name.Gender.Male : Bogus.DataSets.Name.Gender.Female))
                .RuleFor(d => d.LastName, (f, d) => f.Name.LastName(d.Gender == "Male" ? Bogus.DataSets.Name.Gender.Male : Bogus.DataSets.Name.Gender.Female))
                .RuleFor(d => d.MiddleName, f => f.PickRandom(Patronymics))
                .RuleFor(d => d.Birthdate, f => f.Date.Between(new DateTime(1965, 1, 1), new DateTime(2001, 12, 31)).ToString("yyyy-MM-dd"))
                .RuleFor(d => d.PlaceOfBirth, f => f.PickRandom(Cities))
                .RuleFor(d => d.Nationality, _ => "Україна")
                .RuleFor(d => d.MaritalStatus, f => f.PickRandom("Одружений(а)", "Неодружений(а)", "Розлучений(а)"))
                .RuleFor(d => d.IdentificationType, _ => "ID-картка")
                .RuleFor(d => d.IdentificationNumber, f => f.Random.ReplaceNumbers("#########"))
                .RuleFor(d => d.IdentificationExpirationDate, f => f.Date.Future(8).ToString("yyyy-MM-dd"))
                .RuleFor(d => d.Address, f => $"{f.PickRandom(Cities)}, {f.Address.StreetAddress()}")
                .RuleFor(d => d.Phone, f => f.Random.ReplaceNumbers("+38067#######"))
                .RuleFor(d => d.Email, (f, d) => f.Internet.Email(provider: "cargo.ua"))
                .RuleFor(d => d.DriverLicenseNumber, f => f.Random.ReplaceNumbers("DL-######"))
                .RuleFor(d => d.DriverLicenseCategory, f => f.PickRandom("C", "CE", "C1E", "BCE"))
                .RuleFor(d => d.DriverLicenseIssuingDate, f => f.Date.Past(15).ToString("yyyy-MM-dd"))
                .RuleFor(d => d.DriverLicenseExpirationDate, f => f.Date.Future(10).ToString("yyyy-MM-dd"))
                .RuleFor(d => d.DriverLicenseIssuingAuthority, f => f.Random.ReplaceNumbers("ТСЦ МВС № ####"))
                .RuleFor(d => d.EmploymentStatus, f => f.PickRandom("Active", "Active", "Active", "OnLeave", "OnRoute"))
                .RuleFor(d => d.EmploymentStartDate, f => f.Date.Past(10).ToString("yyyy-MM-dd"))
                .RuleFor(d => d.EmploymentEndDate, _ => null);

            // Доменна фабрика Driver.Create — сідинг іде через ту саму модель, що й API
            var drivers = driverFaker.Generate(DriversCount).Select(Driver.Create).ToList();
            await db.Drivers.AddRangeAsync(drivers, stoppingToken);

            // ---------------- 2. Компанії (для ~60% водіїв) ----------------
            var companyFaker = new Faker("uk") { Random = new Randomizer(FakerSeed + 1) };
            foreach (var driver in drivers.Where((_, i) => i % 5 < 3))
            {
                var company = Company.Create(new CompanyForCreation
                {
                    CompanyName = $"ТОВ «{companyFaker.PickRandom(Carriers)}»",
                    Address = $"{companyFaker.PickRandom(Cities)}, {companyFaker.Address.StreetAddress()}",
                    Phone = companyFaker.Random.ReplaceNumbers("+38044#######"),
                    Email = companyFaker.Internet.Email(provider: "logistics.ua"),
                    ContactPerson = companyFaker.Name.FullName(),
                    ContactPhone = companyFaker.Random.ReplaceNumbers("+38050#######"),
                    ContactEmail = companyFaker.Internet.Email(provider: "logistics.ua")
                });
                await db.Companies.AddAsync(company, stoppingToken);

                // Зв'язок тримає тіньова властивість DriverId (див. DriverConfiguration)
                db.Entry(company).Property("DriverId").CurrentValue = driver.Id;
            }

            // ---------------- 3. Вантажівки ----------------
            var truckFaker = new Faker<TruckForCreation>("uk")
                .UseSeed(FakerSeed + 2)
                .RuleFor(t => t.TruckNumber, f => f.Random.ReplaceNumbers("TR-###"))
                .RuleFor(t => t.Model, f => f.PickRandom(TruckModels))
                .RuleFor(t => t.Year, f => f.Random.Int(2012, 2025).ToString())
                .RuleFor(t => t.Capacity, f => $"{f.Random.Int(10, 26)} т")
                .RuleFor(t => t.FuelType, f => f.PickRandom("Дизель", "LNG", "Електро"))
                .RuleFor(t => t.RegistrationNumber, f => $"{f.PickRandom("AA", "BC", "AX", "KA", "BH")}{f.Random.ReplaceNumbers("####")}{f.Random.String2(2, "ABCEHIKMOPTX")}")
                .RuleFor(t => t.VIN, f => f.Vehicle.Vin())
                .RuleFor(t => t.EngineNumber, f => f.Random.AlphaNumeric(10).ToUpperInvariant())
                .RuleFor(t => t.InsurancePolicyNumber, f => f.Random.ReplaceNumbers("ЕР-#########"))
                .RuleFor(t => t.InsuranceInspirationDate, f => f.Date.Future(1).ToString("yyyy-MM-dd"));
            await db.Trucks.AddRangeAsync(truckFaker.Generate(TrucksCount).Select(Truck.Create), stoppingToken);

            await db.SaveChangesAsync(stoppingToken);
            _logger.LogInformation("🌱 Збережено {Drivers} водіїв, компанії та {Trucks} вантажівок.", drivers.Count, TrucksCount);

            // ---------------- 4. Події для реплікації ----------------
            // Чекаємо готовності шини: MassTransit стартує асинхронно разом із хостом.
            await WaitForBusAsync(stoppingToken);

            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
            foreach (var driver in drivers)
            {
                await publishEndpoint.Publish<IDriverCreated>(new DriverCreated
                {
                    DriverId = driver.Id,
                    FirstName = driver.FirstName,
                    LastName = driver.LastName,
                    DriverLicenseNumber = driver.DriverLicenseNumber,
                    Phone = driver.Phone
                }, stoppingToken);
            }

            _logger.LogInformation("🌱 Опубліковано {Count} подій IDriverCreated для реплікації у блог і замовлення.", drivers.Count);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Застосунок зупиняється — сідинг перервано, це нормально
        }
        catch (Exception ex)
        {
            // Сідинг — допоміжна функція: його збій не повинен "класти" сервіс
            _logger.LogError(ex, "🌱 Помилка демо-сідингу DriversManagement.");
        }
    }

    private async Task WaitForBusAsync(CancellationToken stoppingToken)
    {
        for (var attempt = 0; attempt < 60; attempt++)
        {
            if (_bus.CheckHealth().Status == BusHealthStatus.Healthy)
                return;
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
        _logger.LogWarning("🌱 Шина MassTransit не стала Healthy за 60 с — публікуємо без очікування.");
    }
}

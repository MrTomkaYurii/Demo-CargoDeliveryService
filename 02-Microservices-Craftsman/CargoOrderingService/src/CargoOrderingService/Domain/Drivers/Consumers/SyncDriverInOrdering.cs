namespace CargoOrderingService.Domain.Drivers.Consumers;

using CargoOrderingService.Databases;
using CargoOrderingService.Domain.Drivers;
using CargoOrderingService.Domain.Drivers.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Messages;
using System.Threading.Tasks;

/// <summary>
/// ====================================================================================
/// 📬 АРХІТЕКТУРНИЙ ПАТЕРН: MASSTRANSIT CONSUMER У CARGOORDERINGSERVICE
/// ------------------------------------------------------------------------------------
/// АРХІТЕКТУРНА КОНЦЕПЦІЯ:
///
/// 1. ПРИЗНАЧЕННЯ:
///    Споживач MassTransit у мікросервісі замовлень вантажів (CargoOrderingService).
///    Автоматично реплікує сутності водіїв у локальну базу даних Ordering Context
///    при отриманні доменних подій через RabbitMQ Fanout Exchange.
///
/// 2. АВТОНОМНІСТЬ СЕРВІСУ ЗАМОВЛЕНЬ (AUTONOMY):
///    Коли диспетчер призначає водія на вантажну доставку, сервісу CargoOrderingService
///    не потрібно робити мережевий REST або gRPC запит до DriversManagement.
///    Вся необхідна інформація (Id, ПІБ, посвідчення, телефон) вже збережена
///    у власній локальній таблиці Drivers завдяки цьому споживачу.
///    Якщо сервіс DriversManagement тимчасово вимкнено — CargoOrderingService
///    продовжує повноцінно працювати й оформлювати рейси!
///
/// 3. ОБРОБКА ДВОХ ПОДІЙ В ОДНОМУ КЛАСІ:
///    Клас реалізує інтерфейси:
///      - IConsumer&lt;IDriverCreated&gt;
///      - IConsumer&lt;IDriverUpdated&gt;
///    MassTransit самостійно прив'язує обидві черги до відповідних методів Consume().
/// ====================================================================================
/// </summary>
public sealed class SyncDriverInOrdering : IConsumer<IDriverCreated>, IConsumer<IDriverUpdated>
{
    private readonly OrderingContext _db;
    private readonly ILogger<SyncDriverInOrdering> _logger;

    public SyncDriverInOrdering(OrderingContext db, ILogger<SyncDriverInOrdering> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Обробляє подію IDriverCreated та створює запис у локальній БД замовлень.
    /// </summary>
    public async Task Consume(ConsumeContext<IDriverCreated> context)
    {
        var msg = context.Message;
        _logger.LogInformation("==>> [MassTransit Ordering] Отримано IDriverCreated: {DriverId}, {FirstName} {LastName}",
            msg.DriverId, msg.FirstName, msg.LastName);

        var existingDriver = await _db.Drivers.FirstOrDefaultAsync(d => d.Id == msg.DriverId, context.CancellationToken);
        if (existingDriver == null)
        {
            var driver = Driver.CreateWithId(msg.DriverId, msg.FirstName, msg.LastName, msg.DriverLicenseNumber, msg.Phone);
            await _db.Drivers.AddAsync(driver, context.CancellationToken);
            await _db.SaveChangesAsync(context.CancellationToken);
            _logger.LogInformation("==>> [MassTransit Ordering] Створено локальну сутність водія: {DriverId}", msg.DriverId);
        }
        else
        {
            _logger.LogInformation("==>> [MassTransit Ordering] Водій {DriverId} вже існує. Ідемпотентно пропущено.", msg.DriverId);
        }
    }

    /// <summary>
    /// Обробляє подію IDriverUpdated та оновлює локальні реквізити водія.
    /// </summary>
    public async Task Consume(ConsumeContext<IDriverUpdated> context)
    {
        var msg = context.Message;
        _logger.LogInformation("==>> [MassTransit Ordering] Отримано IDriverUpdated: {DriverId}, {FirstName} {LastName}",
            msg.DriverId, msg.FirstName, msg.LastName);

        var existingDriver = await _db.Drivers.FirstOrDefaultAsync(d => d.Id == msg.DriverId, context.CancellationToken);
        if (existingDriver != null)
        {
            existingDriver.Update(new DriverForUpdate
            {
                FirstName = msg.FirstName,
                LastName = msg.LastName
            });
            await _db.SaveChangesAsync(context.CancellationToken);
            _logger.LogInformation("==>> [MassTransit Ordering] Оновлено локальну сутність водія: {DriverId}", msg.DriverId);
        }
        else
        {
            var driver = Driver.CreateWithId(msg.DriverId, msg.FirstName, msg.LastName);
            await _db.Drivers.AddAsync(driver, context.CancellationToken);
            await _db.SaveChangesAsync(context.CancellationToken);
            _logger.LogInformation("==>> [MassTransit Ordering] Створено водія з події оновлення: {DriverId}", msg.DriverId);
        }
    }
}

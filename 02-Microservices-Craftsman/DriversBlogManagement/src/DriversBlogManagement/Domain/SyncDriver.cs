namespace Billing.Domain;

using DriversBlogManagement.Databases;
using DriversBlogManagement.Domain.Drivers;
using DriversBlogManagement.Domain.Drivers.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Messages;
using System.Threading.Tasks;

/// <summary>
/// ====================================================================================
/// 📬 АРХІТЕКТУРНИЙ ПАТЕРН: MASSTRANSIT CONSUMER (EVENT-DRIVEN DATA REPLICATION)
/// ------------------------------------------------------------------------------------
/// АРХІТЕКТУРНА КОНЦЕПЦІЯ:
///
/// 1. ПРИЗНАЧЕННЯ:
///    Асинхронний споживач подій шини повідомлень MassTransit (RabbitMQ).
///    Автоматично реплікує інформацію про водіїв з основного сервісу DriversManagement
///    у локальну базу даних мікросервісу блогів (DriversBlogManagement).
///
/// 2. КОНЦЕПЦІЯ "SHADOW MODEL" (ТІНЬОВА МОДЕЛЬ ВОДІЯ):
///    Мікросервісу блогів НЕ потрібні всі поля водія (паспорт, категорія, медогляд).
///    Йому потрібні лише Id, FirstName та LastName для підпису постів та лайків.
///    Тому блог створює власну спрощену копію сутності Driver у своїй базі (Database-per-Service).
///
/// 3. ПРИНЦИП ІДЕМПОТЕНТНОСТІ (IDEMPOTENCY):
///    У розподілених мережах брокер повідомлень гарантує семантику "At-least-once delivery"
///    (повідомлення доставляється як мінімум один раз, але через мережеві повтори
///    може надійти двічі).
///    Код перевіряє: if (existingDriver == null) -> Add, інакше оновлюємо або ігноруємо.
///    Це захищає базу від дублювання та конфліктів первинних ключів.
///
/// 4. ПІДТВЕРДЖЕННЯ ДОСТАВКИ (ACK / NACK):
///    Коли метод Consume завершується без помилок, MassTransit надсилає брокеру RabbitMQ
///    сигнал Basic.Ack, і повідомлення остаточно видаляється з черги.
///    Якщо трапляється виняток — MassTransit робить повторні спроби (Retry Policy),
///    а при постійному збої переміщує повідомлення у чергу помилок (Error Queue / Dead Letter).
/// ====================================================================================
/// </summary>
public sealed class SyncDriver : IConsumer<IDriverCreated>, IConsumer<IDriverUpdated>
{
    private readonly BlogManagementContext _db;
    private readonly ILogger<SyncDriver> _logger;

    public SyncDriver(BlogManagementContext db, ILogger<SyncDriver> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Обробляє подію створення водія (IDriverCreated).
    /// </summary>
    public async Task Consume(ConsumeContext<IDriverCreated> context)
    {
        var msg = context.Message;
        _logger.LogInformation("==>> [MassTransit Blog] Отримано подію IDriverCreated: DriverId={DriverId}, {FirstName} {LastName}", 
            msg.DriverId, msg.FirstName, msg.LastName);

        // Перевірка на ідемпотентність: чи не існує вже водій з таким Id
        var existingDriver = await _db.Drivers.FirstOrDefaultAsync(d => d.Id == msg.DriverId, context.CancellationToken);
        if (existingDriver == null)
        {
            // Створюємо локальну тіньову модель водія
            var driver = Driver.CreateWithId(msg.DriverId, msg.FirstName, msg.LastName);
            await _db.Drivers.AddAsync(driver, context.CancellationToken);
            await _db.SaveChangesAsync(context.CancellationToken);
            _logger.LogInformation("==>> [MassTransit Blog] Тіньового водія успішно збережено в локальній БД: {DriverId}", msg.DriverId);
        }
        else
        {
            _logger.LogInformation("==>> [MassTransit Blog] Водій {DriverId} вже існує в локальній БД. Повідомлення пропущено (Ідемпотентність).", msg.DriverId);
        }
    }

    /// <summary>
    /// Обробляє подію оновлення даних водія (IDriverUpdated).
    /// </summary>
    public async Task Consume(ConsumeContext<IDriverUpdated> context)
    { 
        var msg = context.Message;
        _logger.LogInformation("==>> [MassTransit Blog] Отримано подію IDriverUpdated: DriverId={DriverId}, {FirstName} {LastName}", 
            msg.DriverId, msg.FirstName, msg.LastName);

        var existingDriver = await _db.Drivers.FirstOrDefaultAsync(d => d.Id == msg.DriverId, context.CancellationToken);
        if (existingDriver != null)
        {
            // Оновлюємо ім'я та прізвище у локальній базі блогу
            existingDriver.Update(new DriverForUpdate
            {
                FirstName = msg.FirstName,
                LastName = msg.LastName
            });
            await _db.SaveChangesAsync(context.CancellationToken);
            _logger.LogInformation("==>> [MassTransit Blog] Тіньового водія успішно оновлено локально: {DriverId}", msg.DriverId);
        }
        else
        {
            // Upsert fallback: якщо подія оновлення прийшла раніше за створення (Out of order)
            var driver = Driver.CreateWithId(msg.DriverId, msg.FirstName, msg.LastName);
            await _db.Drivers.AddAsync(driver, context.CancellationToken);
            await _db.SaveChangesAsync(context.CancellationToken);
            _logger.LogInformation("==>> [MassTransit Blog] Тіньового водія створено з події оновлення: {DriverId}", msg.DriverId);
        }
    }
}

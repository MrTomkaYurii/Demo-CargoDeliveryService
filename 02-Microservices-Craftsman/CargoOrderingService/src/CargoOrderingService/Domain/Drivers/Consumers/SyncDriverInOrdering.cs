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
/// ПАТЕРН EVENT-DRIVEN DATA REPLICATION:
/// Споживач MassTransit у мікросервісі CargoOrderingService.
/// Автоматично реплікує сутності водіїв у локальну базу даних Ordering Context
/// при отриманні подій IDriverCreated та IDriverUpdated через RabbitMQ.
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

    public async Task Consume(ConsumeContext<IDriverCreated> context)
    {
        var msg = context.Message;
        _logger.LogInformation("==>> [Ordering MassTransit] Received IDriverCreated: {DriverId}, {FirstName} {LastName}",
            msg.DriverId, msg.FirstName, msg.LastName);

        var existingDriver = await _db.Drivers.FirstOrDefaultAsync(d => d.Id == msg.DriverId, context.CancellationToken);
        if (existingDriver == null)
        {
            var driver = Driver.CreateWithId(msg.DriverId, msg.FirstName, msg.LastName, msg.DriverLicenseNumber, msg.Phone);
            await _db.Drivers.AddAsync(driver, context.CancellationToken);
            await _db.SaveChangesAsync(context.CancellationToken);
            _logger.LogInformation("==>> [Ordering MassTransit] Created local Driver entity: {DriverId}", msg.DriverId);
        }
    }

    public async Task Consume(ConsumeContext<IDriverUpdated> context)
    {
        var msg = context.Message;
        _logger.LogInformation("==>> [Ordering MassTransit] Received IDriverUpdated: {DriverId}, {FirstName} {LastName}",
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
            _logger.LogInformation("==>> [Ordering MassTransit] Updated local Driver entity: {DriverId}", msg.DriverId);
        }
        else
        {
            var driver = Driver.CreateWithId(msg.DriverId, msg.FirstName, msg.LastName);
            await _db.Drivers.AddAsync(driver, context.CancellationToken);
            await _db.SaveChangesAsync(context.CancellationToken);
            _logger.LogInformation("==>> [Ordering MassTransit] Created Driver from update event: {DriverId}", msg.DriverId);
        }
    }
}

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
/// ПАТЕРН EVENT-DRIVEN DATA REPLICATION / EVENTUAL CONSISTENCY:
/// Асинхронний споживач подій шини повідомлень MassTransit (RabbitMQ).
/// Забезпечує реплікацію довідника водіїв з основного мікросервісу DriversManagement
/// у локальну базу даних мікросервісу блогів (DriversBlogManagement).
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

    public async Task Consume(ConsumeContext<IDriverCreated> context)
    {
        var msg = context.Message;
        _logger.LogInformation("==>> [MassTransit] Received IDriverCreated: DriverId={DriverId}, Name={FirstName} {LastName}", 
            msg.DriverId, msg.FirstName, msg.LastName);

        var existingDriver = await _db.Drivers.FirstOrDefaultAsync(d => d.Id == msg.DriverId, context.CancellationToken);
        if (existingDriver == null)
        {
            var driver = Driver.CreateWithId(msg.DriverId, msg.FirstName, msg.LastName);
            await _db.Drivers.AddAsync(driver, context.CancellationToken);
            await _db.SaveChangesAsync(context.CancellationToken);
            _logger.LogInformation("==>> [MassTransit] Shadow Driver created locally: {DriverId}", msg.DriverId);
        }
    }

    public async Task Consume(ConsumeContext<IDriverUpdated> context)
    { 
        var msg = context.Message;
        _logger.LogInformation("==>> [MassTransit] Received IDriverUpdated: DriverId={DriverId}, Name={FirstName} {LastName}", 
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
            _logger.LogInformation("==>> [MassTransit] Shadow Driver updated locally: {DriverId}", msg.DriverId);
        }
        else
        {
            var driver = Driver.CreateWithId(msg.DriverId, msg.FirstName, msg.LastName);
            await _db.Drivers.AddAsync(driver, context.CancellationToken);
            await _db.SaveChangesAsync(context.CancellationToken);
            _logger.LogInformation("==>> [MassTransit] Shadow Driver created from update event: {DriverId}", msg.DriverId);
        }
    }
}

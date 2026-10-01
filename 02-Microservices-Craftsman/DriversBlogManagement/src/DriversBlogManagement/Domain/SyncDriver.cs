namespace Billing.Domain;

using DriversBlogManagement.Databases;
using MassTransit;
using SharedKernel.Messages;
using System.Threading;
using System.Threading.Tasks;


public sealed class SyncDriver : IConsumer<IDriverUpdated>
{
    private readonly BlogManagementContext _db;

    public SyncDriver(BlogManagementContext db)
    {
        _db = db;
    }

    public Task Consume(ConsumeContext<IDriverUpdated> context)
    { 
        Console.WriteLine("WE GOT MESSAGE FORM QUEUE");

        var resieved_message = context.Message;
        
        //var driverToUpdate = await _driverRepository.GetById(request.DriverId, cancellationToken: cancellationToken);
        //var driverToAdd = request.UpdatedDriverData.ToDriverForUpdate();
        //driverToUpdate.Update(driverToAdd);

        //_driverRepository.Update(driverToUpdate);
        //await _unitOfWork.CommitChanges(cancellationToken);
        // do work here

        Console.WriteLine("JOB IS DONE");
        return Task.CompletedTask;
    }
}
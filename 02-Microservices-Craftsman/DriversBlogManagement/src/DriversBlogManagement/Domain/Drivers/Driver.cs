using DriversBlogManagement.Domain.PostAboutDrivers;
using DriversBlogManagement.Domain.PostAboutDrivers.Models;
using DriversBlogManagement.Domain.Likes;
using DriversBlogManagement.Domain.Drivers.Models;
using DriversBlogManagement.Domain.Drivers.DomainEvents;

namespace DriversBlogManagement.Domain.Drivers;
public class Driver : BaseEntity
{
    public string FirstName { get; private set; }

    public string LastName { get; private set; }

    public IReadOnlyCollection<Like> Likes { get; } = new List<Like>();

    public PostAboutDriver PostAboutDriver { get; private set; } = PostAboutDriver.Create(new PostAboutDriverForCreation());

    // Add Props Marker -- Deleting this comment will cause the add props utility to be incomplete


    public static Driver Create(DriverForCreation driverForCreation)
    {
        var newDriver = new Driver();

        newDriver.FirstName = driverForCreation.FirstName;
        newDriver.LastName = driverForCreation.LastName;

        newDriver.QueueDomainEvent(new DriverCreated(){ Driver = newDriver });
        
        return newDriver;
    }

    public Driver Update(DriverForUpdate driverForUpdate)
    {
        FirstName = driverForUpdate.FirstName;
        LastName = driverForUpdate.LastName;

        QueueDomainEvent(new DriverUpdated(){ Id = Id });
        return this;
    }

    // Add Prop Methods Marker -- Deleting this comment will cause the add props utility to be incomplete
    
    protected Driver() { } // For EF + Mocking
}

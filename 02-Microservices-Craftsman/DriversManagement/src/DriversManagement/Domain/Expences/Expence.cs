using DriversManagement.Domain.Trucks;
using DriversManagement.Domain.Drivers;
using DriversManagement.Domain.Expences.Models;
using DriversManagement.Domain.Expences.DomainEvents;

namespace DriversManagement.Domain.Expences;
public class Expence : BaseEntity
{
    public string DriverPaiment { get; private set; }

    public string FuelCost { get; private set; }

    public string MaintanceCost { get; private set; }

    public string Category { get; private set; }

    public string Date { get; private set; }

    public string Note { get; private set; }

    public Driver Driver { get; private set; }

    public Truck Truck { get; private set; }

    // Add Props Marker -- Deleting this comment will cause the add props utility to be incomplete


    public static Expence Create(ExpenceForCreation expenceForCreation)
    {
        var newExpence = new Expence();

        newExpence.DriverPaiment = expenceForCreation.DriverPaiment;
        newExpence.FuelCost = expenceForCreation.FuelCost;
        newExpence.MaintanceCost = expenceForCreation.MaintanceCost;
        newExpence.Category = expenceForCreation.Category;
        newExpence.Date = expenceForCreation.Date;
        newExpence.Note = expenceForCreation.Note;

        newExpence.QueueDomainEvent(new ExpenceCreated(){ Expence = newExpence });
        
        return newExpence;
    }

    public Expence Update(ExpenceForUpdate expenceForUpdate)
    {
        DriverPaiment = expenceForUpdate.DriverPaiment;
        FuelCost = expenceForUpdate.FuelCost;
        MaintanceCost = expenceForUpdate.MaintanceCost;
        Category = expenceForUpdate.Category;
        Date = expenceForUpdate.Date;
        Note = expenceForUpdate.Note;

        QueueDomainEvent(new ExpenceUpdated(){ Id = Id });
        return this;
    }

    public Expence SetDriver(Driver driver)
    {
        Driver = driver;
        return this;
    }

    public Expence SetTruck(Truck truck)
    {
        Truck = truck;
        return this;
    }

    // Add Prop Methods Marker -- Deleting this comment will cause the add props utility to be incomplete
    
    protected Expence() { } // For EF + Mocking
}

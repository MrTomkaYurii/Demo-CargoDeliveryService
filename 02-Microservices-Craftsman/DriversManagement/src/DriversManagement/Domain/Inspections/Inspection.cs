using DriversManagement.Domain.Trucks;
using DriversManagement.Domain.Inspections.Models;
using DriversManagement.Domain.Inspections.DomainEvents;

namespace DriversManagement.Domain.Inspections;
public class Inspection : BaseEntity
{
    public string InspectionDate { get; private set; }

    public string Description { get; private set; }

    public string Result { get; private set; }

    public Truck Truck { get; private set; }

    // Add Props Marker -- Deleting this comment will cause the add props utility to be incomplete


    public static Inspection Create(InspectionForCreation inspectionForCreation)
    {
        var newInspection = new Inspection();

        newInspection.InspectionDate = inspectionForCreation.InspectionDate;
        newInspection.Description = inspectionForCreation.Description;
        newInspection.Result = inspectionForCreation.Result;

        newInspection.QueueDomainEvent(new InspectionCreated(){ Inspection = newInspection });
        
        return newInspection;
    }

    public Inspection Update(InspectionForUpdate inspectionForUpdate)
    {
        InspectionDate = inspectionForUpdate.InspectionDate;
        Description = inspectionForUpdate.Description;
        Result = inspectionForUpdate.Result;

        QueueDomainEvent(new InspectionUpdated(){ Id = Id });
        return this;
    }

    public Inspection SetTruck(Truck truck)
    {
        Truck = truck;
        return this;
    }

    // Add Prop Methods Marker -- Deleting this comment will cause the add props utility to be incomplete
    
    protected Inspection() { } // For EF + Mocking
}

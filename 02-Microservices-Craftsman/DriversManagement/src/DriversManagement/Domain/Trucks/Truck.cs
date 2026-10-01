namespace DriversManagement.Domain.Trucks;

using System.ComponentModel.DataAnnotations;
using DriversManagement.Domain.Expences;
using DriversManagement.Domain.Inspections;
using DriversManagement.Domain.Repairs;
using System.ComponentModel.DataAnnotations.Schema;
using Destructurama.Attributed;
using DriversManagement.Exceptions;
using DriversManagement.Domain.Trucks.Models;
using DriversManagement.Domain.Trucks.DomainEvents;


public class Truck : BaseEntity
{
    public string TruckNumber { get; private set; }

    public string Model { get; private set; }

    public string Year { get; private set; }

    public string Capacity { get; private set; }

    public string FuelType { get; private set; }

    public string RegistrationNumber { get; private set; }

    public string VIN { get; private set; }

    public string EngineNumber { get; private set; }

    public string InsurancePolicyNumber { get; private set; }

    public string InsuranceInspirationDate { get; private set; }

    public IReadOnlyCollection<Repair> Repairs { get; } = new List<Repair>();

    public IReadOnlyCollection<Inspection> Inspections { get; } = new List<Inspection>();

    public IReadOnlyCollection<Expence> Expences { get; } = new List<Expence>();

    // Add Props Marker -- Deleting this comment will cause the add props utility to be incomplete


    public static Truck Create(TruckForCreation truckForCreation)
    {
        var newTruck = new Truck();

        newTruck.TruckNumber = truckForCreation.TruckNumber;
        newTruck.Model = truckForCreation.Model;
        newTruck.Year = truckForCreation.Year;
        newTruck.Capacity = truckForCreation.Capacity;
        newTruck.FuelType = truckForCreation.FuelType;
        newTruck.RegistrationNumber = truckForCreation.RegistrationNumber;
        newTruck.VIN = truckForCreation.VIN;
        newTruck.EngineNumber = truckForCreation.EngineNumber;
        newTruck.InsurancePolicyNumber = truckForCreation.InsurancePolicyNumber;
        newTruck.InsuranceInspirationDate = truckForCreation.InsuranceInspirationDate;

        newTruck.QueueDomainEvent(new TruckCreated(){ Truck = newTruck });
        
        return newTruck;
    }

    public Truck Update(TruckForUpdate truckForUpdate)
    {
        TruckNumber = truckForUpdate.TruckNumber;
        Model = truckForUpdate.Model;
        Year = truckForUpdate.Year;
        Capacity = truckForUpdate.Capacity;
        FuelType = truckForUpdate.FuelType;
        RegistrationNumber = truckForUpdate.RegistrationNumber;
        VIN = truckForUpdate.VIN;
        EngineNumber = truckForUpdate.EngineNumber;
        InsurancePolicyNumber = truckForUpdate.InsurancePolicyNumber;
        InsuranceInspirationDate = truckForUpdate.InsuranceInspirationDate;

        QueueDomainEvent(new TruckUpdated(){ Id = Id });
        return this;
    }

    // Add Prop Methods Marker -- Deleting this comment will cause the add props utility to be incomplete
    
    protected Truck() { } // For EF + Mocking
}

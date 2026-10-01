namespace DriversManagement.SharedTestHelpers.Fakes.Truck;

using DriversManagement.Domain.Trucks;
using DriversManagement.Domain.Trucks.Models;

public class FakeTruckBuilder
{
    private TruckForCreation _creationData = new FakeTruckForCreation().Generate();

    public FakeTruckBuilder WithModel(TruckForCreation model)
    {
        _creationData = model;
        return this;
    }
    
    public FakeTruckBuilder WithTruckNumber(string truckNumber)
    {
        _creationData.TruckNumber = truckNumber;
        return this;
    }
    
    public FakeTruckBuilder WithModel(string model)
    {
        _creationData.Model = model;
        return this;
    }
    
    public FakeTruckBuilder WithYear(string year)
    {
        _creationData.Year = year;
        return this;
    }
    
    public FakeTruckBuilder WithCapacity(string capacity)
    {
        _creationData.Capacity = capacity;
        return this;
    }
    
    public FakeTruckBuilder WithFuelType(string fuelType)
    {
        _creationData.FuelType = fuelType;
        return this;
    }
    
    public FakeTruckBuilder WithRegistrationNumber(string registrationNumber)
    {
        _creationData.RegistrationNumber = registrationNumber;
        return this;
    }
    
    public FakeTruckBuilder WithVIN(string vIN)
    {
        _creationData.VIN = vIN;
        return this;
    }
    
    public FakeTruckBuilder WithEngineNumber(string engineNumber)
    {
        _creationData.EngineNumber = engineNumber;
        return this;
    }
    
    public FakeTruckBuilder WithInsurancePolicyNumber(string insurancePolicyNumber)
    {
        _creationData.InsurancePolicyNumber = insurancePolicyNumber;
        return this;
    }
    
    public FakeTruckBuilder WithInsuranceInspirationDate(string insuranceInspirationDate)
    {
        _creationData.InsuranceInspirationDate = insuranceInspirationDate;
        return this;
    }
    
    public Truck Build()
    {
        var result = Truck.Create(_creationData);
        return result;
    }
}
namespace DriversManagement.SharedTestHelpers.Fakes.Expence;

using DriversManagement.Domain.Expences;
using DriversManagement.Domain.Expences.Models;

public class FakeExpenceBuilder
{
    private ExpenceForCreation _creationData = new FakeExpenceForCreation().Generate();

    public FakeExpenceBuilder WithModel(ExpenceForCreation model)
    {
        _creationData = model;
        return this;
    }
    
    public FakeExpenceBuilder WithDriverPaiment(string driverPaiment)
    {
        _creationData.DriverPaiment = driverPaiment;
        return this;
    }
    
    public FakeExpenceBuilder WithFuelCost(string fuelCost)
    {
        _creationData.FuelCost = fuelCost;
        return this;
    }
    
    public FakeExpenceBuilder WithMaintanceCost(string maintanceCost)
    {
        _creationData.MaintanceCost = maintanceCost;
        return this;
    }
    
    public FakeExpenceBuilder WithCategory(string category)
    {
        _creationData.Category = category;
        return this;
    }
    
    public FakeExpenceBuilder WithDate(string date)
    {
        _creationData.Date = date;
        return this;
    }
    
    public FakeExpenceBuilder WithNote(string note)
    {
        _creationData.Note = note;
        return this;
    }
    
    public Expence Build()
    {
        var result = Expence.Create(_creationData);
        return result;
    }
}
namespace DriversManagement.SharedTestHelpers.Fakes.Repair;

using DriversManagement.Domain.Repairs;
using DriversManagement.Domain.Repairs.Models;

public class FakeRepairBuilder
{
    private RepairForCreation _creationData = new FakeRepairForCreation().Generate();

    public FakeRepairBuilder WithModel(RepairForCreation model)
    {
        _creationData = model;
        return this;
    }
    
    public FakeRepairBuilder WithRepairDate(string repairDate)
    {
        _creationData.RepairDate = repairDate;
        return this;
    }
    
    public FakeRepairBuilder WithDescription(string description)
    {
        _creationData.Description = description;
        return this;
    }
    
    public FakeRepairBuilder WithCost(string cost)
    {
        _creationData.Cost = cost;
        return this;
    }
    
    public Repair Build()
    {
        var result = Repair.Create(_creationData);
        return result;
    }
}
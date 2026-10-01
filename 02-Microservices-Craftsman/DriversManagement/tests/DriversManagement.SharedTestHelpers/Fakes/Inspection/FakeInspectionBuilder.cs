namespace DriversManagement.SharedTestHelpers.Fakes.Inspection;

using DriversManagement.Domain.Inspections;
using DriversManagement.Domain.Inspections.Models;

public class FakeInspectionBuilder
{
    private InspectionForCreation _creationData = new FakeInspectionForCreation().Generate();

    public FakeInspectionBuilder WithModel(InspectionForCreation model)
    {
        _creationData = model;
        return this;
    }
    
    public FakeInspectionBuilder WithInspectionDate(string inspectionDate)
    {
        _creationData.InspectionDate = inspectionDate;
        return this;
    }
    
    public FakeInspectionBuilder WithDescription(string description)
    {
        _creationData.Description = description;
        return this;
    }
    
    public FakeInspectionBuilder WithResult(string result)
    {
        _creationData.Result = result;
        return this;
    }
    
    public Inspection Build()
    {
        var result = Inspection.Create(_creationData);
        return result;
    }
}
namespace DriversBlogManagement.SharedTestHelpers.Fakes.Driver;

using DriversBlogManagement.Domain.Drivers;
using DriversBlogManagement.Domain.Drivers.Models;

public class FakeDriverBuilder
{
    private DriverForCreation _creationData = new FakeDriverForCreation().Generate();

    public FakeDriverBuilder WithModel(DriverForCreation model)
    {
        _creationData = model;
        return this;
    }
    
    public FakeDriverBuilder WithFirstName(string firstName)
    {
        _creationData.FirstName = firstName;
        return this;
    }
    
    public FakeDriverBuilder WithLastName(string lastName)
    {
        _creationData.LastName = lastName;
        return this;
    }
    
    public Driver Build()
    {
        var result = Driver.Create(_creationData);
        return result;
    }
}
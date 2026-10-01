namespace DriversManagement.UnitTests.Domain.Inspections;

using DriversManagement.SharedTestHelpers.Fakes.Inspection;
using DriversManagement.Domain.Inspections;
using DriversManagement.Domain.Inspections.DomainEvents;
using Bogus;
using FluentAssertions.Extensions;
using ValidationException = DriversManagement.Exceptions.ValidationException;

public class CreateInspectionTests
{
    private readonly Faker _faker;

    public CreateInspectionTests()
    {
        _faker = new Faker();
    }
    
    [Fact]
    public void can_create_valid_inspection()
    {
        // Arrange
        var inspectionToCreate = new FakeInspectionForCreation().Generate();
        
        // Act
        var inspection = Inspection.Create(inspectionToCreate);

        // Assert
        inspection.InspectionDate.Should().Be(inspectionToCreate.InspectionDate);
        inspection.Description.Should().Be(inspectionToCreate.Description);
        inspection.Result.Should().Be(inspectionToCreate.Result);
    }

    [Fact]
    public void queue_domain_event_on_create()
    {
        // Arrange
        var inspectionToCreate = new FakeInspectionForCreation().Generate();
        
        // Act
        var inspection = Inspection.Create(inspectionToCreate);

        // Assert
        inspection.DomainEvents.Count.Should().Be(1);
        inspection.DomainEvents.FirstOrDefault().Should().BeOfType(typeof(InspectionCreated));
    }
}
namespace DriversManagement.UnitTests.Domain.Inspections;

using DriversManagement.SharedTestHelpers.Fakes.Inspection;
using DriversManagement.Domain.Inspections;
using DriversManagement.Domain.Inspections.DomainEvents;
using Bogus;
using FluentAssertions.Extensions;
using ValidationException = DriversManagement.Exceptions.ValidationException;

public class UpdateInspectionTests
{
    private readonly Faker _faker;

    public UpdateInspectionTests()
    {
        _faker = new Faker();
    }
    
    [Fact]
    public void can_update_inspection()
    {
        // Arrange
        var inspection = new FakeInspectionBuilder().Build();
        var updatedInspection = new FakeInspectionForUpdate().Generate();
        
        // Act
        inspection.Update(updatedInspection);

        // Assert
        inspection.InspectionDate.Should().Be(updatedInspection.InspectionDate);
        inspection.Description.Should().Be(updatedInspection.Description);
        inspection.Result.Should().Be(updatedInspection.Result);
    }
    
    [Fact]
    public void queue_domain_event_on_update()
    {
        // Arrange
        var inspection = new FakeInspectionBuilder().Build();
        var updatedInspection = new FakeInspectionForUpdate().Generate();
        inspection.DomainEvents.Clear();
        
        // Act
        inspection.Update(updatedInspection);

        // Assert
        inspection.DomainEvents.Count.Should().Be(1);
        inspection.DomainEvents.FirstOrDefault().Should().BeOfType(typeof(InspectionUpdated));
    }
}
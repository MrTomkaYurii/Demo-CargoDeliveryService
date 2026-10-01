namespace DriversManagement.UnitTests.Domain.Repairs;

using DriversManagement.SharedTestHelpers.Fakes.Repair;
using DriversManagement.Domain.Repairs;
using DriversManagement.Domain.Repairs.DomainEvents;
using Bogus;
using FluentAssertions.Extensions;
using ValidationException = DriversManagement.Exceptions.ValidationException;

public class UpdateRepairTests
{
    private readonly Faker _faker;

    public UpdateRepairTests()
    {
        _faker = new Faker();
    }
    
    [Fact]
    public void can_update_repair()
    {
        // Arrange
        var repair = new FakeRepairBuilder().Build();
        var updatedRepair = new FakeRepairForUpdate().Generate();
        
        // Act
        repair.Update(updatedRepair);

        // Assert
        repair.RepairDate.Should().Be(updatedRepair.RepairDate);
        repair.Description.Should().Be(updatedRepair.Description);
        repair.Cost.Should().Be(updatedRepair.Cost);
    }
    
    [Fact]
    public void queue_domain_event_on_update()
    {
        // Arrange
        var repair = new FakeRepairBuilder().Build();
        var updatedRepair = new FakeRepairForUpdate().Generate();
        repair.DomainEvents.Clear();
        
        // Act
        repair.Update(updatedRepair);

        // Assert
        repair.DomainEvents.Count.Should().Be(1);
        repair.DomainEvents.FirstOrDefault().Should().BeOfType(typeof(RepairUpdated));
    }
}
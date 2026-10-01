namespace DriversManagement.UnitTests.Domain.Repairs;

using DriversManagement.SharedTestHelpers.Fakes.Repair;
using DriversManagement.Domain.Repairs;
using DriversManagement.Domain.Repairs.DomainEvents;
using Bogus;
using FluentAssertions.Extensions;
using ValidationException = DriversManagement.Exceptions.ValidationException;

public class CreateRepairTests
{
    private readonly Faker _faker;

    public CreateRepairTests()
    {
        _faker = new Faker();
    }
    
    [Fact]
    public void can_create_valid_repair()
    {
        // Arrange
        var repairToCreate = new FakeRepairForCreation().Generate();
        
        // Act
        var repair = Repair.Create(repairToCreate);

        // Assert
        repair.RepairDate.Should().Be(repairToCreate.RepairDate);
        repair.Description.Should().Be(repairToCreate.Description);
        repair.Cost.Should().Be(repairToCreate.Cost);
    }

    [Fact]
    public void queue_domain_event_on_create()
    {
        // Arrange
        var repairToCreate = new FakeRepairForCreation().Generate();
        
        // Act
        var repair = Repair.Create(repairToCreate);

        // Assert
        repair.DomainEvents.Count.Should().Be(1);
        repair.DomainEvents.FirstOrDefault().Should().BeOfType(typeof(RepairCreated));
    }
}
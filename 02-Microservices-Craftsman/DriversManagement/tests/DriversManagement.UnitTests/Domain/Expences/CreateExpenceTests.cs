namespace DriversManagement.UnitTests.Domain.Expences;

using DriversManagement.SharedTestHelpers.Fakes.Expence;
using DriversManagement.Domain.Expences;
using DriversManagement.Domain.Expences.DomainEvents;
using Bogus;
using FluentAssertions.Extensions;
using ValidationException = DriversManagement.Exceptions.ValidationException;

public class CreateExpenceTests
{
    private readonly Faker _faker;

    public CreateExpenceTests()
    {
        _faker = new Faker();
    }
    
    [Fact]
    public void can_create_valid_expence()
    {
        // Arrange
        var expenceToCreate = new FakeExpenceForCreation().Generate();
        
        // Act
        var expence = Expence.Create(expenceToCreate);

        // Assert
        expence.DriverPaiment.Should().Be(expenceToCreate.DriverPaiment);
        expence.FuelCost.Should().Be(expenceToCreate.FuelCost);
        expence.MaintanceCost.Should().Be(expenceToCreate.MaintanceCost);
        expence.Category.Should().Be(expenceToCreate.Category);
        expence.Date.Should().Be(expenceToCreate.Date);
        expence.Note.Should().Be(expenceToCreate.Note);
    }

    [Fact]
    public void queue_domain_event_on_create()
    {
        // Arrange
        var expenceToCreate = new FakeExpenceForCreation().Generate();
        
        // Act
        var expence = Expence.Create(expenceToCreate);

        // Assert
        expence.DomainEvents.Count.Should().Be(1);
        expence.DomainEvents.FirstOrDefault().Should().BeOfType(typeof(ExpenceCreated));
    }
}
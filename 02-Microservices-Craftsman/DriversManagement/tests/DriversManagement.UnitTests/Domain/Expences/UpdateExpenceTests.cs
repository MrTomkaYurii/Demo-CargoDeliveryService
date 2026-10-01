namespace DriversManagement.UnitTests.Domain.Expences;

using DriversManagement.SharedTestHelpers.Fakes.Expence;
using DriversManagement.Domain.Expences;
using DriversManagement.Domain.Expences.DomainEvents;
using Bogus;
using FluentAssertions.Extensions;
using ValidationException = DriversManagement.Exceptions.ValidationException;

public class UpdateExpenceTests
{
    private readonly Faker _faker;

    public UpdateExpenceTests()
    {
        _faker = new Faker();
    }
    
    [Fact]
    public void can_update_expence()
    {
        // Arrange
        var expence = new FakeExpenceBuilder().Build();
        var updatedExpence = new FakeExpenceForUpdate().Generate();
        
        // Act
        expence.Update(updatedExpence);

        // Assert
        expence.DriverPaiment.Should().Be(updatedExpence.DriverPaiment);
        expence.FuelCost.Should().Be(updatedExpence.FuelCost);
        expence.MaintanceCost.Should().Be(updatedExpence.MaintanceCost);
        expence.Category.Should().Be(updatedExpence.Category);
        expence.Date.Should().Be(updatedExpence.Date);
        expence.Note.Should().Be(updatedExpence.Note);
    }
    
    [Fact]
    public void queue_domain_event_on_update()
    {
        // Arrange
        var expence = new FakeExpenceBuilder().Build();
        var updatedExpence = new FakeExpenceForUpdate().Generate();
        expence.DomainEvents.Clear();
        
        // Act
        expence.Update(updatedExpence);

        // Assert
        expence.DomainEvents.Count.Should().Be(1);
        expence.DomainEvents.FirstOrDefault().Should().BeOfType(typeof(ExpenceUpdated));
    }
}
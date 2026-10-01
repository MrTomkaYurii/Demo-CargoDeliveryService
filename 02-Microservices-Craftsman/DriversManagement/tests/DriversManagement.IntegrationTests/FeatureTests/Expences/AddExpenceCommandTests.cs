namespace DriversManagement.IntegrationTests.FeatureTests.Expences;

using DriversManagement.SharedTestHelpers.Fakes.Expence;
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using DriversManagement.Domain.Expences.Features;

public class AddExpenceCommandTests : TestBase
{
    [Fact]
    public async Task can_add_new_expence_to_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var expenceOne = new FakeExpenceForCreationDto().Generate();

        // Act
        var command = new AddExpence.Command(expenceOne);
        var expenceReturned = await testingServiceScope.SendAsync(command);
        var expenceCreated = await testingServiceScope.ExecuteDbContextAsync(db => db.Expences
            .FirstOrDefaultAsync(e => e.Id == expenceReturned.Id));

        // Assert
        expenceReturned.DriverPaiment.Should().Be(expenceOne.DriverPaiment);
        expenceReturned.FuelCost.Should().Be(expenceOne.FuelCost);
        expenceReturned.MaintanceCost.Should().Be(expenceOne.MaintanceCost);
        expenceReturned.Category.Should().Be(expenceOne.Category);
        expenceReturned.Date.Should().Be(expenceOne.Date);
        expenceReturned.Note.Should().Be(expenceOne.Note);

        expenceCreated.DriverPaiment.Should().Be(expenceOne.DriverPaiment);
        expenceCreated.FuelCost.Should().Be(expenceOne.FuelCost);
        expenceCreated.MaintanceCost.Should().Be(expenceOne.MaintanceCost);
        expenceCreated.Category.Should().Be(expenceOne.Category);
        expenceCreated.Date.Should().Be(expenceOne.Date);
        expenceCreated.Note.Should().Be(expenceOne.Note);
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanAddExpence);
        var expenceOne = new FakeExpenceForCreationDto();

        // Act
        var command = new AddExpence.Command(expenceOne);
        var act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}
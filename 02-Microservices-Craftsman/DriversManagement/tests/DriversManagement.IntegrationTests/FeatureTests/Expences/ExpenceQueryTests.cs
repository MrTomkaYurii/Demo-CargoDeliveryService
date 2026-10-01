namespace DriversManagement.IntegrationTests.FeatureTests.Expences;

using DriversManagement.SharedTestHelpers.Fakes.Expence;
using DriversManagement.Domain.Expences.Features;
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class ExpenceQueryTests : TestBase
{
    [Fact]
    public async Task can_get_existing_expence_with_accurate_props()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var expenceOne = new FakeExpenceBuilder().Build();
        await testingServiceScope.InsertAsync(expenceOne);

        // Act
        var query = new GetExpence.Query(expenceOne.Id);
        var expence = await testingServiceScope.SendAsync(query);

        // Assert
        expence.DriverPaiment.Should().Be(expenceOne.DriverPaiment);
        expence.FuelCost.Should().Be(expenceOne.FuelCost);
        expence.MaintanceCost.Should().Be(expenceOne.MaintanceCost);
        expence.Category.Should().Be(expenceOne.Category);
        expence.Date.Should().Be(expenceOne.Date);
        expence.Note.Should().Be(expenceOne.Note);
    }

    [Fact]
    public async Task get_expence_throws_notfound_exception_when_record_does_not_exist()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var badId = Guid.NewGuid();

        // Act
        var query = new GetExpence.Query(badId);
        Func<Task> act = () => testingServiceScope.SendAsync(query);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanReadExpence);

        // Act
        var command = new GetExpence.Query(Guid.NewGuid());
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}
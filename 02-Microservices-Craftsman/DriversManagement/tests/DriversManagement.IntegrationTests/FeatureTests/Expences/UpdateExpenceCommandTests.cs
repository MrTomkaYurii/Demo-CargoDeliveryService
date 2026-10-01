namespace DriversManagement.IntegrationTests.FeatureTests.Expences;

using DriversManagement.SharedTestHelpers.Fakes.Expence;
using DriversManagement.Domain.Expences.Dtos;
using DriversManagement.Domain.Expences.Features;
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class UpdateExpenceCommandTests : TestBase
{
    [Fact]
    public async Task can_update_existing_expence_in_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var expence = new FakeExpenceBuilder().Build();
        await testingServiceScope.InsertAsync(expence);
        var updatedExpenceDto = new FakeExpenceForUpdateDto().Generate();

        // Act
        var command = new UpdateExpence.Command(expence.Id, updatedExpenceDto);
        await testingServiceScope.SendAsync(command);
        var updatedExpence = await testingServiceScope
            .ExecuteDbContextAsync(db => db.Expences
                .FirstOrDefaultAsync(e => e.Id == expence.Id));

        // Assert
        updatedExpence.DriverPaiment.Should().Be(updatedExpenceDto.DriverPaiment);
        updatedExpence.FuelCost.Should().Be(updatedExpenceDto.FuelCost);
        updatedExpence.MaintanceCost.Should().Be(updatedExpenceDto.MaintanceCost);
        updatedExpence.Category.Should().Be(updatedExpenceDto.Category);
        updatedExpence.Date.Should().Be(updatedExpenceDto.Date);
        updatedExpence.Note.Should().Be(updatedExpenceDto.Note);
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanUpdateExpence);
        var expenceOne = new FakeExpenceForUpdateDto();

        // Act
        var command = new UpdateExpence.Command(Guid.NewGuid(), expenceOne);
        var act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}
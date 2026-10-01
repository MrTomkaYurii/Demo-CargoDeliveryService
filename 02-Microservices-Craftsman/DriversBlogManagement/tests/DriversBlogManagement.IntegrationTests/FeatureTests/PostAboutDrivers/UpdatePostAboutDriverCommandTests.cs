namespace DriversBlogManagement.IntegrationTests.FeatureTests.PostAboutDrivers;

using DriversBlogManagement.SharedTestHelpers.Fakes.PostAboutDriver;
using DriversBlogManagement.Domain.PostAboutDrivers.Dtos;
using DriversBlogManagement.Domain.PostAboutDrivers.Features;
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class UpdatePostAboutDriverCommandTests : TestBase
{
    [Fact]
    public async Task can_update_existing_postaboutdriver_in_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var postAboutDriver = new FakePostAboutDriverBuilder().Build();
        await testingServiceScope.InsertAsync(postAboutDriver);
        var updatedPostAboutDriverDto = new FakePostAboutDriverForUpdateDto().Generate();

        // Act
        var command = new UpdatePostAboutDriver.Command(postAboutDriver.Id, updatedPostAboutDriverDto);
        await testingServiceScope.SendAsync(command);
        var updatedPostAboutDriver = await testingServiceScope
            .ExecuteDbContextAsync(db => db.PostAboutDrivers
                .FirstOrDefaultAsync(p => p.Id == postAboutDriver.Id));

        // Assert
        updatedPostAboutDriver.Title.Should().Be(updatedPostAboutDriverDto.Title);
        updatedPostAboutDriver.Content.Should().Be(updatedPostAboutDriverDto.Content);
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanUpdatePostAboutDriver);
        var postAboutDriverOne = new FakePostAboutDriverForUpdateDto();

        // Act
        var command = new UpdatePostAboutDriver.Command(Guid.NewGuid(), postAboutDriverOne);
        var act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}
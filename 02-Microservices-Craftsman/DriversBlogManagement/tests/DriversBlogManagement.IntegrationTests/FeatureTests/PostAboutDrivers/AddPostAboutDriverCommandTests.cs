namespace DriversBlogManagement.IntegrationTests.FeatureTests.PostAboutDrivers;

using DriversBlogManagement.SharedTestHelpers.Fakes.PostAboutDriver;
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using DriversBlogManagement.Domain.PostAboutDrivers.Features;

public class AddPostAboutDriverCommandTests : TestBase
{
    [Fact]
    public async Task can_add_new_postaboutdriver_to_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var postAboutDriverOne = new FakePostAboutDriverForCreationDto().Generate();

        // Act
        var command = new AddPostAboutDriver.Command(postAboutDriverOne);
        var postAboutDriverReturned = await testingServiceScope.SendAsync(command);
        var postAboutDriverCreated = await testingServiceScope.ExecuteDbContextAsync(db => db.PostAboutDrivers
            .FirstOrDefaultAsync(p => p.Id == postAboutDriverReturned.Id));

        // Assert
        postAboutDriverReturned.Title.Should().Be(postAboutDriverOne.Title);
        postAboutDriverReturned.Content.Should().Be(postAboutDriverOne.Content);

        postAboutDriverCreated.Title.Should().Be(postAboutDriverOne.Title);
        postAboutDriverCreated.Content.Should().Be(postAboutDriverOne.Content);
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanAddPostAboutDriver);
        var postAboutDriverOne = new FakePostAboutDriverForCreationDto();

        // Act
        var command = new AddPostAboutDriver.Command(postAboutDriverOne);
        var act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}
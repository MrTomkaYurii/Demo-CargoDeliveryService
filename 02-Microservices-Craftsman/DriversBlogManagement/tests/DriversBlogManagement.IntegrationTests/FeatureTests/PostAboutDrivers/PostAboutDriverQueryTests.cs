namespace DriversBlogManagement.IntegrationTests.FeatureTests.PostAboutDrivers;

using DriversBlogManagement.SharedTestHelpers.Fakes.PostAboutDriver;
using DriversBlogManagement.Domain.PostAboutDrivers.Features;
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class PostAboutDriverQueryTests : TestBase
{
    [Fact]
    public async Task can_get_existing_postaboutdriver_with_accurate_props()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var postAboutDriverOne = new FakePostAboutDriverBuilder().Build();
        await testingServiceScope.InsertAsync(postAboutDriverOne);

        // Act
        var query = new GetPostAboutDriver.Query(postAboutDriverOne.Id);
        var postAboutDriver = await testingServiceScope.SendAsync(query);

        // Assert
        postAboutDriver.Title.Should().Be(postAboutDriverOne.Title);
        postAboutDriver.Content.Should().Be(postAboutDriverOne.Content);
    }

    [Fact]
    public async Task get_postaboutdriver_throws_notfound_exception_when_record_does_not_exist()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var badId = Guid.NewGuid();

        // Act
        var query = new GetPostAboutDriver.Query(badId);
        Func<Task> act = () => testingServiceScope.SendAsync(query);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanReadPostAboutDriver);

        // Act
        var command = new GetPostAboutDriver.Query(Guid.NewGuid());
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}
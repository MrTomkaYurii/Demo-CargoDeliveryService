namespace DriversBlogManagement.IntegrationTests.FeatureTests.PostAboutDrivers;

using DriversBlogManagement.SharedTestHelpers.Fakes.PostAboutDriver;
using DriversBlogManagement.Domain.PostAboutDrivers.Features;
using Microsoft.EntityFrameworkCore;
using Domain;
using System.Threading.Tasks;

public class DeletePostAboutDriverCommandTests : TestBase
{
    [Fact]
    public async Task can_delete_postaboutdriver_from_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var postAboutDriver = new FakePostAboutDriverBuilder().Build();
        await testingServiceScope.InsertAsync(postAboutDriver);

        // Act
        var command = new DeletePostAboutDriver.Command(postAboutDriver.Id);
        await testingServiceScope.SendAsync(command);
        var postAboutDriverResponse = await testingServiceScope
            .ExecuteDbContextAsync(db => db.PostAboutDrivers
                .CountAsync(p => p.Id == postAboutDriver.Id));

        // Assert
        postAboutDriverResponse.Should().Be(0);
    }

    [Fact]
    public async Task delete_postaboutdriver_throws_notfoundexception_when_record_does_not_exist()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var badId = Guid.NewGuid();

        // Act
        var command = new DeletePostAboutDriver.Command(badId);
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task can_softdelete_postaboutdriver_from_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var postAboutDriver = new FakePostAboutDriverBuilder().Build();
        await testingServiceScope.InsertAsync(postAboutDriver);

        // Act
        var command = new DeletePostAboutDriver.Command(postAboutDriver.Id);
        await testingServiceScope.SendAsync(command);
        var deletedPostAboutDriver = await testingServiceScope.ExecuteDbContextAsync(db => db.PostAboutDrivers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == postAboutDriver.Id));

        // Assert
        deletedPostAboutDriver?.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanDeletePostAboutDriver);

        // Act
        var command = new DeletePostAboutDriver.Command(Guid.NewGuid());
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}
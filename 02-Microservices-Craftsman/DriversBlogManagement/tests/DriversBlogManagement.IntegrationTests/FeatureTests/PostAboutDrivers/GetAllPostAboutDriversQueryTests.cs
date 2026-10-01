namespace DriversBlogManagement.IntegrationTests.FeatureTests.PostAboutDrivers;

using DriversBlogManagement.Domain.PostAboutDrivers.Dtos;
using DriversBlogManagement.SharedTestHelpers.Fakes.PostAboutDriver;
using DriversBlogManagement.Domain.PostAboutDrivers.Features;
using Domain;
using System.Threading.Tasks;

public class GetAllPostAboutDriversQueryTests : TestBase
{
    
    [Fact]
    public async Task can_get_all_postaboutdrivers()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var postAboutDriverOne = new FakePostAboutDriverBuilder().Build();
        var postAboutDriverTwo = new FakePostAboutDriverBuilder().Build();

        await testingServiceScope.InsertAsync(postAboutDriverOne, postAboutDriverTwo);

        // Act
        var query = new GetAllPostAboutDrivers.Query();
        var postAboutDrivers = await testingServiceScope.SendAsync(query);

        // Assert
        postAboutDrivers.Count.Should().BeGreaterThanOrEqualTo(2);
        postAboutDrivers.FirstOrDefault(x => x.Id == postAboutDriverOne.Id).Should().NotBeNull();
        postAboutDrivers.FirstOrDefault(x => x.Id == postAboutDriverTwo.Id).Should().NotBeNull();
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanReadPostAboutDrivers);

        // Act
        var query = new GetAllPostAboutDrivers.Query();
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}
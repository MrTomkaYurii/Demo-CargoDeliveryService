namespace DriversBlogManagement.IntegrationTests.FeatureTests.PostAboutDrivers;

using DriversBlogManagement.Domain.PostAboutDrivers.Dtos;
using DriversBlogManagement.SharedTestHelpers.Fakes.PostAboutDriver;
using DriversBlogManagement.Domain.PostAboutDrivers.Features;
using Domain;
using System.Threading.Tasks;

public class PostAboutDriverListQueryTests : TestBase
{
    
    [Fact]
    public async Task can_get_postaboutdriver_list()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var postAboutDriverOne = new FakePostAboutDriverBuilder().Build();
        var postAboutDriverTwo = new FakePostAboutDriverBuilder().Build();
        var queryParameters = new PostAboutDriverParametersDto();

        await testingServiceScope.InsertAsync(postAboutDriverOne, postAboutDriverTwo);

        // Act
        var query = new GetPostAboutDriverList.Query(queryParameters);
        var postAboutDrivers = await testingServiceScope.SendAsync(query);

        // Assert
        postAboutDrivers.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanReadPostAboutDrivers);
        var queryParameters = new PostAboutDriverParametersDto();

        // Act
        var command = new GetPostAboutDriverList.Query(queryParameters);
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}
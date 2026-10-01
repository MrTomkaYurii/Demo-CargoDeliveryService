namespace CargoOrderingService.IntegrationTests.FeatureTests.Users;

using CargoOrderingService.SharedTestHelpers.Fakes.User;
using CargoOrderingService.Domain.Users.Features;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class UserQueryTests : TestBase
{
    [Fact]
    public async Task can_get_existing_user_with_accurate_props()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var userOne = new FakeUserBuilder().Build();
        await testingServiceScope.InsertAsync(userOne);

        // Act
        var query = new GetUser.Query(userOne.Id);
        var user = await testingServiceScope.SendAsync(query);

        // Assert
        user.FirstName.Should().Be(userOne.FirstName);
        user.LastName.Should().Be(userOne.LastName);
        user.Username.Should().Be(userOne.Username);
        user.Identifier.Should().Be(userOne.Identifier);
        user.Email.Should().Be(userOne.Email.Value);
    }

    [Fact]
    public async Task get_user_throws_notfound_exception_when_record_does_not_exist()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var badId = Guid.NewGuid();

        // Act
        var query = new GetUser.Query(badId);
        Func<Task> act = () => testingServiceScope.SendAsync(query);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
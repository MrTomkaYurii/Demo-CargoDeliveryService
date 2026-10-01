namespace DriversManagement.IntegrationTests.FeatureTests.Users;

using DriversManagement.SharedTestHelpers.Fakes.User;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using DriversManagement.Domain.Users.Features;

public class AddUserCommandTests : TestBase
{
    [Fact]
    public async Task can_add_new_user_to_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var userOne = new FakeUserForCreationDto().Generate();

        // Act
        var command = new AddUser.Command(userOne);
        var userReturned = await testingServiceScope.SendAsync(command);
        var userCreated = await testingServiceScope.ExecuteDbContextAsync(db => db.Users
            .FirstOrDefaultAsync(u => u.Id == userReturned.Id));

        // Assert
        userReturned.FirstName.Should().Be(userOne.FirstName);
        userReturned.LastName.Should().Be(userOne.LastName);
        userReturned.Username.Should().Be(userOne.Username);
        userReturned.Identifier.Should().Be(userOne.Identifier);
        userReturned.Email.Should().Be(userOne.Email);

        userCreated?.FirstName.Should().Be(userOne.FirstName);
        userCreated?.LastName.Should().Be(userOne.LastName);
        userCreated?.Username.Should().Be(userOne.Username);
        userCreated?.Identifier.Should().Be(userOne.Identifier);
        userCreated?.Email.Value.Should().Be(userOne.Email);
    }
}
namespace DriversManagement.FunctionalTests.FunctionalTests.Users;

using DriversManagement.SharedTestHelpers.Fakes.User;
using DriversManagement.FunctionalTests.TestUtilities;
using DriversManagement.Domain;
using System.Net;
using System.Threading.Tasks;

public class UpdateUserRecordTests : TestBase
{
    [Fact]
    public async Task put_user_returns_nocontent_when_entity_exists_and_auth_credentials_are_valid()
    {
        // Arrange
        var user = new FakeUserBuilder().Build();
        var updatedUserDto = new FakeUserForUpdateDto().Generate();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);
        await InsertAsync(user);

        // Act
        var route = ApiRoutes.Users.Put(user.Id);
        var result = await FactoryClient.PutJsonRequestAsync(route, updatedUserDto);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
            
    [Fact]
    public async Task put_user_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var user = new FakeUserBuilder().Build();
        var updatedUserDto = new FakeUserForUpdateDto { }.Generate();

        // Act
        var route = ApiRoutes.Users.Put(user.Id);
        var result = await FactoryClient.PutJsonRequestAsync(route, updatedUserDto);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task put_user_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var user = new FakeUserBuilder().Build();
        var updatedUserDto = new FakeUserForUpdateDto { }.Generate();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.Users.Put(user.Id);
        var result = await FactoryClient.PutJsonRequestAsync(route, updatedUserDto);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
namespace DriversManagement.IntegrationTests.FeatureTests.RolePermissions;

using DriversManagement.Domain.RolePermissions.Dtos;
using DriversManagement.SharedTestHelpers.Fakes.RolePermission;
using DriversManagement.Domain.RolePermissions.Features;
using Domain;
using System.Threading.Tasks;

public class RolePermissionListQueryTests : TestBase
{
    
    [Fact]
    public async Task can_get_rolepermission_list()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var rolePermissionOne = new FakeRolePermissionBuilder().Build();
        var rolePermissionTwo = new FakeRolePermissionBuilder().Build();
        var queryParameters = new RolePermissionParametersDto();

        await testingServiceScope.InsertAsync(rolePermissionOne, rolePermissionTwo);

        // Act
        var query = new GetRolePermissionList.Query(queryParameters);
        var rolePermissions = await testingServiceScope.SendAsync(query);

        // Assert
        rolePermissions.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanReadRolePermissions);
        var queryParameters = new RolePermissionParametersDto();

        // Act
        var command = new GetRolePermissionList.Query(queryParameters);
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}
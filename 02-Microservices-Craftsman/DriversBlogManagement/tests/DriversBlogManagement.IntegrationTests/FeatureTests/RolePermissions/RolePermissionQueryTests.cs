namespace DriversBlogManagement.IntegrationTests.FeatureTests.RolePermissions;

using DriversBlogManagement.SharedTestHelpers.Fakes.RolePermission;
using DriversBlogManagement.Domain.RolePermissions.Features;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class RolePermissionQueryTests : TestBase
{
    [Fact]
    public async Task can_get_existing_rolepermission_with_accurate_props()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var rolePermissionOne = new FakeRolePermissionBuilder().Build();
        await testingServiceScope.InsertAsync(rolePermissionOne);

        // Act
        var query = new GetRolePermission.Query(rolePermissionOne.Id);
        var rolePermission = await testingServiceScope.SendAsync(query);

        // Assert
        rolePermission.Permission.Should().Be(rolePermissionOne.Permission);
        rolePermission.Role.Should().Be(rolePermissionOne.Role.Value);
    }

    [Fact]
    public async Task get_rolepermission_throws_notfound_exception_when_record_does_not_exist()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var badId = Guid.NewGuid();

        // Act
        var query = new GetRolePermission.Query(badId);
        Func<Task> act = () => testingServiceScope.SendAsync(query);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
namespace DriversManagement.UnitTests.Domain.RolePermissions;

using DriversManagement.Domain;
using DriversManagement.Domain.RolePermissions;
using DriversManagement.Wrappers;
using DriversManagement.Domain.RolePermissions.Models;
using DriversManagement.Domain.Roles;
using Bogus;
using ValidationException = DriversManagement.Exceptions.ValidationException;

public class UpdateRolePermissionTests
{
    private readonly Faker _faker;

    public UpdateRolePermissionTests()
    {
        _faker = new Faker();
    }
    
    [Fact]
    public void can_update_rolepermission()
    {
        // Arrange
        var rolePermission = RolePermission.Create(new RolePermissionForCreation()
        {
            Permission = _faker.PickRandom(Permissions.List()),
            Role = _faker.PickRandom(Role.ListNames())
        });
        var permission = _faker.PickRandom(Permissions.List());
        var role = _faker.PickRandom(Role.ListNames());
        
        // Act
        rolePermission.Update(new RolePermissionForUpdate()
        {
            Permission = permission,
            Role = role
        });
        
        // Assert
        rolePermission.Permission.Should().Be(permission);
        rolePermission.Role.Value.Should().Be(role);
    }
    
    [Fact]
    public void can_NOT_update_rolepermission_with_invalid_role()
    {
        // Arrange
        var rolePermission = RolePermission.Create(new RolePermissionForCreation()
        {
            Permission = _faker.PickRandom(Permissions.List()),
            Role = _faker.PickRandom(Role.ListNames())
        });
        var updateRolePermission = () => rolePermission.Update(new RolePermissionForUpdate()
        {
            Permission = _faker.PickRandom(Permissions.List()),
            Role = _faker.Lorem.Word()
        });

        // Act + Assert
        updateRolePermission.Should().Throw<InvalidSmartEnumPropertyName>();
    }
    
    [Fact]
    public void can_NOT_update_rolepermission_with_invalid_permission()
    {
        // Arrange
        var rolePermission = RolePermission.Create(new RolePermissionForCreation()
        {
            Permission = _faker.PickRandom(Permissions.List()),
            Role = _faker.PickRandom(Role.ListNames())
        });
        var updateRolePermission = () => rolePermission.Update(new RolePermissionForUpdate()
        {
            Permission = _faker.Lorem.Word(),
            Role = _faker.PickRandom(Role.ListNames())
        });

        // Act + Assert
        updateRolePermission.Should().Throw<ValidationException>();
    }
}
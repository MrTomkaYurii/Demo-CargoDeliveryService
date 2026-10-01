namespace DriversBlogManagement.SharedTestHelpers.Fakes.RolePermission;

using AutoBogus;
using DriversBlogManagement.Domain;
using DriversBlogManagement.Domain.RolePermissions.Dtos;
using DriversBlogManagement.Domain.Roles;
using DriversBlogManagement.Domain.RolePermissions.Models;

public sealed class FakeRolePermissionForCreation : AutoFaker<RolePermissionForCreation>
{
    public FakeRolePermissionForCreation()
    {
        RuleFor(rp => rp.Permission, f => f.PickRandom(Permissions.List()));
        RuleFor(rp => rp.Role, f => f.PickRandom(Role.ListNames()));
    }
}
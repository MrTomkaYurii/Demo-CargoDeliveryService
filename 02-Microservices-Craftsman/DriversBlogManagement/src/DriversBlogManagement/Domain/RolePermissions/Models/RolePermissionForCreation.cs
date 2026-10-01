namespace DriversBlogManagement.Domain.RolePermissions.Models;

using Destructurama.Attributed;

public sealed class RolePermissionForCreation
{
    public string Role { get; set; }
    public string Permission { get; set; }
}

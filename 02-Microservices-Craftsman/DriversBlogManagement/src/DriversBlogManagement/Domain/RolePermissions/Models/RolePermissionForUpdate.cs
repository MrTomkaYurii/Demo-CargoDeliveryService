namespace DriversBlogManagement.Domain.RolePermissions.Models;

using Destructurama.Attributed;

public sealed class RolePermissionForUpdate
{
    public string Role { get; set; }
    public string Permission { get; set; }
}

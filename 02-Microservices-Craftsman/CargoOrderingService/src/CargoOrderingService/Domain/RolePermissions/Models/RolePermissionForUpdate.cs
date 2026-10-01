namespace CargoOrderingService.Domain.RolePermissions.Models;

using Destructurama.Attributed;

public sealed record RolePermissionForUpdate
{
    public string Role { get; set; }
    public string Permission { get; set; }
}

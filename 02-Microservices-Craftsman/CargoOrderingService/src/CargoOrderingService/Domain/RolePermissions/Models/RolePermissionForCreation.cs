namespace CargoOrderingService.Domain.RolePermissions.Models;

using Destructurama.Attributed;

public sealed record RolePermissionForCreation
{
    public string Role { get; set; }
    public string Permission { get; set; }
}

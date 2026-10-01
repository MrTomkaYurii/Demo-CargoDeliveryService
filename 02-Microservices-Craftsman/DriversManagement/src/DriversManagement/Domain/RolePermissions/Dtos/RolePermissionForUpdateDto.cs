namespace DriversManagement.Domain.RolePermissions.Dtos;

using Destructurama.Attributed;

public sealed record RolePermissionForUpdateDto
{
    public string Role { get; set; }
    public string Permission { get; set; }
}

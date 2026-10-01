namespace DriversBlogManagement.Domain.RolePermissions.Dtos;

using Destructurama.Attributed;

public sealed record RolePermissionForCreationDto
{
    public string Role { get; set; }
    public string Permission { get; set; }
}

namespace CargoOrderingService.Domain.RolePermissions.Dtos;

using Destructurama.Attributed;

public sealed record RolePermissionDto
{
    public Guid Id { get; set; }
    public string Role { get; set; }
    public string Permission { get; set; }
}

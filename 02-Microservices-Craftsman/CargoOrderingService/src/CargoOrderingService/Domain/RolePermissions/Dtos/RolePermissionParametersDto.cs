namespace CargoOrderingService.Domain.RolePermissions.Dtos;

using CargoOrderingService.Resources;

public sealed class RolePermissionParametersDto : BasePaginationParameters
{
    public string? Filters { get; set; }
    public string? SortOrder { get; set; }
}

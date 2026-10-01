namespace DriversManagement.Domain.RolePermissions.Dtos;

using DriversManagement.Resources;

public sealed class RolePermissionParametersDto : BasePaginationParameters
{
    public string? Filters { get; set; }
    public string? SortOrder { get; set; }
}

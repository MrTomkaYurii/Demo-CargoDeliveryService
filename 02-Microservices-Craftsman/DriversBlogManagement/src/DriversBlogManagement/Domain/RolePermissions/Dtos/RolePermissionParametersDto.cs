namespace DriversBlogManagement.Domain.RolePermissions.Dtos;

using DriversBlogManagement.Resources;

public sealed class RolePermissionParametersDto : BasePaginationParameters
{
    public string? Filters { get; set; }
    public string? SortOrder { get; set; }
}

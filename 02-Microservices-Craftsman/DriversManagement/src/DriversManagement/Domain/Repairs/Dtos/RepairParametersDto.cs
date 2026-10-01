namespace DriversManagement.Domain.Repairs.Dtos;

using DriversManagement.Resources;

public sealed class RepairParametersDto : BasePaginationParameters
{
    public string? Filters { get; set; }
    public string? SortOrder { get; set; }
}

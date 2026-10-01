namespace DriversManagement.Domain.Inspections.Dtos;

using DriversManagement.Resources;

public sealed class InspectionParametersDto : BasePaginationParameters
{
    public string? Filters { get; set; }
    public string? SortOrder { get; set; }
}

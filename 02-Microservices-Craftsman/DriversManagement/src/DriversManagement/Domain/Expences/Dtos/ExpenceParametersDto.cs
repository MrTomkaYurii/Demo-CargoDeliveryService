namespace DriversManagement.Domain.Expences.Dtos;

using DriversManagement.Resources;

public sealed class ExpenceParametersDto : BasePaginationParameters
{
    public string? Filters { get; set; }
    public string? SortOrder { get; set; }
}

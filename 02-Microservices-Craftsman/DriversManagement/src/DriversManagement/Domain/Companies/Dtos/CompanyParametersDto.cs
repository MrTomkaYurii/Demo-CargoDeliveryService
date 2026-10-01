namespace DriversManagement.Domain.Companies.Dtos;

using DriversManagement.Resources;

public sealed class CompanyParametersDto : BasePaginationParameters
{
    public string? Filters { get; set; }
    public string? SortOrder { get; set; }
}

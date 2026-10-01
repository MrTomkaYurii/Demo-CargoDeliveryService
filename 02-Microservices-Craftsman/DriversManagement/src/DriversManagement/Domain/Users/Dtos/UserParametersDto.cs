namespace DriversManagement.Domain.Users.Dtos;

using DriversManagement.Resources;

public sealed class UserParametersDto : BasePaginationParameters
{
    public string? Filters { get; set; }
    public string? SortOrder { get; set; }
}

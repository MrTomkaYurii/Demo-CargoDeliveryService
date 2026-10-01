namespace DriversBlogManagement.Domain.Users.Dtos;

using DriversBlogManagement.Resources;

public sealed class UserParametersDto : BasePaginationParameters
{
    public string? Filters { get; set; }
    public string? SortOrder { get; set; }
}

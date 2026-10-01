namespace CargoOrderingService.Domain.Users.Dtos;

using CargoOrderingService.Resources;

public sealed class UserParametersDto : BasePaginationParameters
{
    public string? Filters { get; set; }
    public string? SortOrder { get; set; }
}

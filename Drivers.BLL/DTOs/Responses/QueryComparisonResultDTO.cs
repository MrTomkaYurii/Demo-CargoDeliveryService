namespace Drivers.BLL.DTOs.Responses;

/// <summary>
/// DTO результатів бенчмаркінгу та порівняння продуктивності між EF Core та Dapper
/// </summary>
/// <typeparam name="T">Тип повернених даних</typeparam>
public class QueryComparisonResultDTO<T>
{
    public string QueryName { get; set; } = default!;
    public T? Result { get; set; }
    public double EntityFrameworkExecutionTimeMs { get; set; }
    public double DapperExecutionTimeMs { get; set; }
    public string FasterProvider { get; set; } = default!;
    public double SpeedupFactor { get; set; }
    public bool DataConsistent { get; set; }
    public string? Notes { get; set; }
}

namespace CargoDelivery.HttpAggregator.Domain.Dtos;

/// <summary>
/// Результат виконання демонстраційного виклику стійкості (Polly Resilience Demo).
/// </summary>
public sealed class ResilienceDemoResultDto
{
    public string Scenario { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int AttemptCount { get; set; }
    public long ElapsedMilliseconds { get; set; }
    public string Details { get; set; } = string.Empty;
}

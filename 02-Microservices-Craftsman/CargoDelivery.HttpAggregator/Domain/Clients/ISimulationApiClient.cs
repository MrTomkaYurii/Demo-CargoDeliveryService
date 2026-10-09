namespace CargoDelivery.HttpAggregator.Domain.Clients;

/// <summary>
/// Імена окремих HttpClient-ів лабораторії стійкості.
/// <para>
/// Кожен сценарій має ВЛАСНИЙ іменований клієнт, а отже — власний екземпляр конвеєра Polly
/// і власний стан Circuit Breaker. Інакше збої одного демо (наприклад, /timeout) розмикали б
/// ланцюг для іншого (/retry), і результати залежали б від порядку запуску експериментів.
/// </para>
/// </summary>
public static class SimulationClients
{
    public const string Retry = "simulation-retry";
    public const string Timeout = "simulation-timeout";
    public const string CircuitBreaker = "simulation-circuit-breaker";
}

/// <summary>
/// Інтерфейс демо-клієнта до власного SimulationController агрегатора.
/// Використовується лише лабораторією стійкості (ResilienceDemoController).
/// </summary>
public interface ISimulationApiClient
{
    /// <summary>
    /// Виконує GET-запит до /api/simulation/{relativePath} через конвеєр Polly обраного сценарію.
    /// </summary>
    /// <param name="clientName">Ім'я клієнта зі <see cref="SimulationClients"/> (визначає політики Polly).</param>
    /// <param name="selfBaseAddress">Базова адреса самого агрегатора (схема + хост + порт).</param>
    /// <param name="relativePath">Шлях на кшталт "flaky?failUntil=2", "slow?delaySeconds=5" або "error".</param>
    /// <param name="cancellationToken">Токен скасування запиту.</param>
    Task<string> GetAsync(string clientName, Uri selfBaseAddress, string relativePath, CancellationToken cancellationToken = default);
}

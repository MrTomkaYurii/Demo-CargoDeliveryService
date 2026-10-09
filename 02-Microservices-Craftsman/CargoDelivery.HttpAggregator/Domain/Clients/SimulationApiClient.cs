namespace CargoDelivery.HttpAggregator.Domain.Clients;

/// <summary>
/// ====================================================================================
/// 🧪 SIMULATION API CLIENT (ЛАБОРАТОРІЯ POLLY)
/// ------------------------------------------------------------------------------------
/// ПРИЗНАЧЕННЯ:
/// Звертається до SimulationController ЦЬОГО Ж агрегатора (ендпоінти /slow, /flaky,
/// /error), проходячи крізь реальний мережевий стек: SocketsHttpHandler, конвеєр Polly,
/// інструментацію OpenTelemetry. Тому в Aspire Dashboard кожну спробу Polly видно як
/// окремий дочірній HTTP-спан.
///
/// ЧОМУ IHttpClientFactory, А НЕ ТИПІЗОВАНИЙ HttpClient?
/// Для кожного сценарію зареєстровано ОКРЕМИЙ іменований клієнт (див. Program.cs, розділ 3)
/// зі своїми параметрами Polly та своїм Circuit Breaker. Фабрика видає потрібний клієнт за
/// іменем: _httpClientFactory.CreateClient(SimulationClients.Retry).
///
/// ЧОМУ АДРЕСА ПЕРЕДАЄТЬСЯ ПАРАМЕТРОМ?
/// Агрегатор не має посилання сам на себе в AppHost (.WithReference на себе створив би
/// цикл), тож логічного імені для Service Discovery немає. Контролер знає фактичну
/// адресу з поточного запиту (Request.Scheme + Request.Host) і передає її сюди.
/// ====================================================================================
/// </summary>
public sealed class SimulationApiClient : ISimulationApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SimulationApiClient> _logger;

    public SimulationApiClient(IHttpClientFactory httpClientFactory, ILogger<SimulationApiClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<string> GetAsync(string clientName, Uri selfBaseAddress, string relativePath, CancellationToken cancellationToken = default)
    {
        // CreateClient дешевий: важкий SocketsHttpHandler і конвеєр Polly фабрика кешує за іменем
        var httpClient = _httpClientFactory.CreateClient(clientName);
        var requestUri = new Uri(selfBaseAddress, $"api/simulation/{relativePath}");
        _logger.LogInformation("Демо-виклик [{Client}] через конвеєр Polly: GET {RequestUri}", clientName, requestUri);

        // Увесь конвеєр Polly (Retry, Timeout, Circuit Breaker) працює ВСЕРЕДИНІ цього виклику.
        // Якщо всі спроби вичерпано — EnsureSuccessStatusCode кине HttpRequestException;
        // якщо спрацював таймаут — Polly кине TimeoutRejectedException;
        // якщо ланцюг розімкнено — BrokenCircuitException (без виходу в мережу).
        var response = await httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}

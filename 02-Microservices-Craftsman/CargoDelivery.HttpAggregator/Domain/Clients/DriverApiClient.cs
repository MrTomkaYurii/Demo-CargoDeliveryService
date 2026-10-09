namespace CargoDelivery.HttpAggregator.Domain.Clients;

using System.Net;
using System.Net.Http.Json;
using CargoDelivery.HttpAggregator.Domain.Dtos;
using Microsoft.Extensions.Logging;

/// <summary>
/// ====================================================================================
/// 🚚 DRIVER API TYPED HTTP CLIENT (.NET 10 & IHttpClientFactory)
/// ------------------------------------------------------------------------------------
/// ПРИЗНАЧЕННЯ:
/// Надає типізований доступ до сервісу DriversManagement за логічним іменем:
/// "https+http://driversmanagement" (див. Program.cs).
///
/// ASPIRE SERVICE DISCOVERY & POLLY RESILIENCE:
/// 1. Логічне ім'я "driversmanagement" підміняється на реальний хост/порт через Service Discovery.
/// 2. Клієнт захищений стандартним конвеєром Polly (Retry, Circuit Breaker, Timeout),
///    який глобально додає ServiceDefaults (ConfigureHttpClientDefaults).
///
/// РОЛЬ В АГРЕГАЦІЇ:
/// Водій — КОРІНЬ агрегації і ЖОРСТКА залежність: без нього зведення не має сенсу.
/// Тому мережеві збої тут НЕ ковтаються, а прокидаються вище (контролер поверне 503).
/// ====================================================================================
/// </summary>
public sealed class DriverApiClient : IDriverApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DriverApiClient> _logger;

    // HttpClient приходить з IHttpClientFactory: сам об'єкт легкий (transient), а важкий
    // SocketsHttpHandler із пулом з'єднань фабрика перевикористовує і періодично оновлює.
    public DriverApiClient(HttpClient httpClient, ILogger<DriverApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<AggregatedDriverDto?> GetDriverByIdAsync(Guid driverId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Надсилання запиту GET до DriversManagement для водія {DriverId}", driverId);

        try
        {
            // Відносний шлях: BaseAddress уже містить https+http://driversmanagement.
            // Внутрішній (service-to-service) ендпоінт повертає компактну картку водія.
            var response = await _httpClient.GetAsync($"api/internal/drivers/{driverId}", cancellationToken);

            // 404 — очікуваний БІЗНЕС-результат ("водія не існує"), а не збій інфраструктури.
            // Обробляємо його явно, без дорогого винятку. До того ж стандартний Retry Polly
            // не повторює 404 (повторюються лише транзієнтні 408/429/5xx та мережеві збої).
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Водія {DriverId} не знайдено в DriversManagement (404)", driverId);
                return null;
            }

            // Для інших неуспішних кодів (401, 500, 503 після всіх повторів...) кидаємо HttpRequestException
            response.EnsureSuccessStatusCode();

            // Потокова десеріалізація System.Text.Json напряму з мережевого Stream
            return await response.Content.ReadFromJsonAsync<AggregatedDriverDto>(cancellationToken: cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Мережевий збій при зверненні до DriversManagement для водія {DriverId}", driverId);
            throw;
        }
    }
}

namespace DriversManagement.Domain.Drivers.Services.Http;

using System.Net.Http.Json;
using System.Text.Json;
using DriversManagement.Domain.Drivers.Dtos;
using Microsoft.Extensions.Logging;

/// <summary>
/// ====================================================================================
/// 🌐 РЕАЛІЗАЦІЯ ТИПІЗОВАНОГО HTTP-КЛІЄНТА ДО CARGOORDERINGSERVICE
/// ------------------------------------------------------------------------------------
/// ЯК ЦЕ ПРАЦЮЄ ПІД КАПОТОМ (.NET 10 &amp; .NET ASPIRE):
///
/// 1. ЧОМУ МИ НЕ ВИКОРИСТОВУЄМО "new HttpClient()"?
///    Створення екземплярів через "new" призводить до вичерпання сокетів операційної
///    системи (Socket Exhaustion під навантаженням). Натомість цей клас реєструється через:
///    builder.Services.AddHttpClient&lt;ICargoOrderingApiClient, CargoOrderingApiClient&gt;(...)
///    Фабрика IHttpClientFactory автоматично пулить і перевикористовує базові обробники
///    HttpMessageHandler, уникаючи витоків ресурсів.
///
/// 2. ЗВІДКИ БЕРЕТЬСЯ АДРЕСА СЕРВІСУ (SERVICE DISCOVERY)?
///    У Program.cs встановлюється: client.BaseAddress = new Uri("http://cargoorderingservice");
///    У середовищі .NET Aspire компонент Service Discovery автоматично перехоплює це
///    логічне ім'я "cargoorderingservice" і підставляє реальний IP-порт запущеного контейнера
///    чи локального процесу (наприклад, http://localhost:5241).
///
/// 3. СТІЙКІСТЬ ЧЕРЕЗ POLLY (.AddStandardResilienceHandler):
///    Усі виклики через цей клієнт автоматично захищені:
///    - Повторними спробами (Retry з експоненційним бекоффом при HTTP 503/504).
///    - Розмикачем ланцюга (Circuit Breaker при масових збоях).
///    - Загальним таймаутом на виконання запиту.
/// ====================================================================================
/// </summary>
public sealed class CargoOrderingApiClient : ICargoOrderingApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CargoOrderingApiClient> _logger;

    /// <summary>
    /// Конструктор отримує сконфігурований HttpClient із пулу IHttpClientFactory.
    /// </summary>
    public CargoOrderingApiClient(HttpClient httpClient, ILogger<CargoOrderingApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Виконує GET-запит до ендпоінту доставок у CargoOrderingService.
    /// </summary>
    public async Task<List<AggregatedDeliveryDto>> GetActiveDeliveriesByDriverAsync(
        Guid driverId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Формуємо відносний шлях. Базова адреса вже містить http://cargoorderingservice/
            var requestUri = $"api/v1/deliveries";

            _logger.LogInformation("Надсилання HTTP GET запиту до {BaseAddress}{Uri} для водія {DriverId}",
                _httpClient.BaseAddress, requestUri, driverId);

            // Використовуємо оптимізований метод GetFromJsonAsync із підтримкою CancellationToken
            var response = await _httpClient.GetFromJsonAsync<JsonElement>(
                requestUri, 
                cancellationToken);

            var result = new List<AggregatedDeliveryDto>();

            // Парсимо JSON відповідь від мікросервісу замовлень
            if (response.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in response.EnumerateArray())
                {
                    result.Add(new AggregatedDeliveryDto
                    {
                        Id = item.TryGetProperty("id", out var idProp) && Guid.TryParse(idProp.GetString(), out var g) ? g : Guid.NewGuid(),
                        Status = item.TryGetProperty("status", out var sProp) ? sProp.GetString() ?? "Active" : "Active",
                        RouteDescription = item.TryGetProperty("destination", out var dProp) ? dProp.GetString() : "Вантажний маршрут"
                    });
                }
            }

            return result;
        }
        catch (HttpRequestException ex)
        {
            // Логуємо попередження для діагностики в OpenTelemetry.
            // Ми не викидаємо виняток вище, оскільки контролер Агрегатора застосовує
            // патерн Graceful Degradation і не повинен падати при збої одного сервісу.
            _logger.LogWarning(ex, 
                "Не вдалося отримати доставки з CargoOrderingService для водія {DriverId}. Статус відповіді: {StatusCode}", 
                driverId, ex.StatusCode);
            
            throw; // Прокидаємо для централізованої обробки в SafeFetchAsync
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Запит до CargoOrderingService було скасовано за таймаутом або запитом користувача.");
            throw;
        }
    }
}

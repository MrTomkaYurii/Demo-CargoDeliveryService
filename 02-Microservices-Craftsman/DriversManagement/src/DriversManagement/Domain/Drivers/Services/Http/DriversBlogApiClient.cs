namespace DriversManagement.Domain.Drivers.Services.Http;

using System.Net.Http.Json;
using System.Text.Json;
using DriversManagement.Domain.Drivers.Dtos;
using Microsoft.Extensions.Logging;

/// <summary>
/// ====================================================================================
/// 🌐 РЕАЛІЗАЦІЯ ТИПІЗОВАНОГО HTTP-КЛІЄНТА ДО DRIVERSBLOGMANAGEMENT
/// ------------------------------------------------------------------------------------
/// АРХІТЕКТУРНІ ОСОБЛИВОСТІ:
///
/// 1. ВЗАЄМОДІЯ З СЕРВІСОМ ЧЕРЕЗ .NET ASPIRE SERVICE DISCOVERY:
///    Адресація сервісу налаштовується як "http://driversblogmanagement".
///    У продакшені або при тестуванні в Aspire AppHost ім'я автоматично мапиться
///    на внутрішню адресу відповідного контейнера/процесу.
///
/// 2. РОБОТА З ТЕЛЕМЕТРІЄЮ (OPENTELEMETRY):
///    Клієнт бере участь у розподіленому трейсингу: заголовки "traceparent"
///    прокидаються автоматично завдяки HttpClientInstrumentation, що дає змогу
///    бачити цей HTTP-виклик як дочірній спан у дашборді Aspire.
///
/// 3. ОБРОБКА НЕСТАБІЛЬНОСТІ МЕРЕЖІ:
///    Якщо сервіс блогів відповідає із затримкою або повертає помилку 500,
///    обробник Polly Standard Resilience робить повторні спроби, а при збереженні
///    збою генерує виняток, який перехоплюється вищим рівнем для повернення дефолтних даних.
/// ====================================================================================
/// </summary>
public sealed class DriversBlogApiClient : IDriversBlogApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DriversBlogApiClient> _logger;

    public DriversBlogApiClient(HttpClient httpClient, ILogger<DriversBlogApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Отримує пости про водія через REST API ендпоінт /api/v1/postaboutdrivers.
    /// </summary>
    public async Task<List<AggregatedBlogPostDto>> GetDriverPostsAsync(
        Guid driverId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var requestUri = "api/v1/postaboutdrivers";

            _logger.LogInformation("Надсилання HTTP GET запиту до {BaseAddress}{Uri} для водія {DriverId}",
                _httpClient.BaseAddress, requestUri, driverId);

            var response = await _httpClient.GetFromJsonAsync<JsonElement>(
                requestUri, 
                cancellationToken);

            var result = new List<AggregatedBlogPostDto>();

            if (response.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in response.EnumerateArray())
                {
                    result.Add(new AggregatedBlogPostDto
                    {
                        Id = item.TryGetProperty("id", out var idProp) && Guid.TryParse(idProp.GetString(), out var g) ? g : Guid.NewGuid(),
                        Title = item.TryGetProperty("title", out var tProp) ? tProp.GetString() ?? "Без назви" : "Без назви",
                        Content = item.TryGetProperty("description", out var cProp) ? cProp.GetString() ?? "" : ""
                    });
                }
            }

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, 
                "Не вдалося отримати пости з DriversBlogManagement для водія {DriverId}. Статус: {StatusCode}", 
                driverId, ex.StatusCode);
            
            throw; // Прокидаємо для обробки у SafeFetchAsync всередині CQRS Handler
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Запит до DriversBlogManagement скасовано.");
            throw;
        }
    }
}

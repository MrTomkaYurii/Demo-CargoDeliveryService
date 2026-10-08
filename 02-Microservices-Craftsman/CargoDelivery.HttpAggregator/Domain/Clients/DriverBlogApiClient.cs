namespace CargoDelivery.HttpAggregator.Domain.Clients;

using System.Net;
using System.Net.Http.Json;
using CargoDelivery.HttpAggregator.Domain.Dtos;
using Microsoft.Extensions.Logging;

/// <summary>
/// ====================================================================================
/// 📝 DRIVER BLOG API TYPED HTTP CLIENT (.NET 10 & IHttpClientFactory)
/// ------------------------------------------------------------------------------------
/// ПРИЗНАЧЕННЯ:
/// Опитує мікросервіс DriversBlogManagement за адресою "http://driversblogmanagement".
/// Використовується як для бізнес-отримання нотаток водія, так і для демонстрації
/// політик Polly (Retry, Timeout, Circuit Breaker).
/// ====================================================================================
/// </summary>
public sealed class DriverBlogApiClient : IDriverBlogApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DriverBlogApiClient> _logger;

    public DriverBlogApiClient(HttpClient httpClient, ILogger<DriverBlogApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<AggregatedBlogNoteDto>> GetRoadNotesByDriverIdAsync(
        Guid driverId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Отримання дорожніх заміток блогу для водія {DriverId}", driverId);

        try
        {
            var response = await _httpClient.GetAsync($"api/postaboutdrivers?driverId={driverId}", cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return [];
            }

            response.EnsureSuccessStatusCode();

            var notes = await response.Content.ReadFromJsonAsync<List<AggregatedBlogNoteDto>>(cancellationToken: cancellationToken);
            return notes ?? [];
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Помилка зв'язку з DriversBlogManagement для водія {DriverId}. Активовано Graceful Degradation.", driverId);
            // Кидаємо виняток, щоб агрегатор зафіксував факт деградації у ShipmentSummaryService
            throw;
        }
    }

    public async Task<string> ExecuteResilienceProbeAsync(string mode, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Виконання зонду стійкості (Resilience Probe) у режимі '{Mode}'", mode);

        // Звертаємося до ендпоінту симуляції
        var response = await _httpClient.GetAsync($"api/simulation/{mode}", cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}

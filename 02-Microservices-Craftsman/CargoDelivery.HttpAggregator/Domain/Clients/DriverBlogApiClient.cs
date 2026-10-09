namespace CargoDelivery.HttpAggregator.Domain.Clients;

using System.Net.Http.Json;
using CargoDelivery.HttpAggregator.Domain.Dtos;
using Microsoft.Extensions.Logging;

/// <summary>
/// ====================================================================================
/// 📝 DRIVER BLOG API TYPED HTTP CLIENT (.NET 10 & IHttpClientFactory)
/// ------------------------------------------------------------------------------------
/// ПРИЗНАЧЕННЯ:
/// Опитує мікросервіс DriversBlogManagement за адресою "https+http://driversblogmanagement"
/// і повертає "дорожні замітки" водія (останні пости блогу).
///
/// РОЛЬ В АГРЕГАЦІЇ:
/// Блог — М'ЯКА (некритична) залежність. Клієнт сам НЕ вирішує, що робити при збої:
/// він прокидає виняток, а рішення про Graceful Degradation приймає
/// ShipmentAggregatorService (SafeFetchRoadNotesAsync) — там, де відомий бізнес-контекст.
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
            var notes = await _httpClient.GetFromJsonAsync<List<AggregatedBlogNoteDto>>(
                $"api/internal/postaboutdrivers?driverId={driverId}", cancellationToken);

            return notes ?? [];
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Помилка зв'язку з DriversBlogManagement для водія {DriverId}.", driverId);
            // Прокидаємо виняток, щоб агрегатор зафіксував факт деградації (IsDegraded = true)
            throw;
        }
    }
}

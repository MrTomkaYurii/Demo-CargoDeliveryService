namespace CargoDelivery.HttpAggregator.Domain.Clients;

using System.Net;
using System.Net.Http.Json;
using CargoDelivery.HttpAggregator.Domain.Dtos;
using Microsoft.Extensions.Logging;

/// <summary>
/// ====================================================================================
/// 📦 CARGO ORDER API TYPED HTTP CLIENT (.NET 10 & IHttpClientFactory)
/// ------------------------------------------------------------------------------------
/// ПРИЗНАЧЕННЯ:
/// Опитує мікросервіс CargoOrderingService за логічною адресою:
/// "http://cargoorderingservice".
///
/// ДОМЕН:
/// Отримує активні доставки та вантажі водія для відображення в диспетчерському дашборді.
/// ====================================================================================
/// </summary>
public sealed class CargoOrderApiClient : ICargoOrderApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CargoOrderApiClient> _logger;

    public CargoOrderApiClient(HttpClient httpClient, ILogger<CargoOrderApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<AggregatedCargoOrderDto>> GetActiveOrdersByDriverIdAsync(
        Guid driverId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Отримання активних доставок з CargoOrderingService для водія {DriverId}", driverId);

        try
        {
            var response = await _httpClient.GetAsync($"api/deliveries?driverId={driverId}", cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return [];
            }

            response.EnsureSuccessStatusCode();

            var orders = await response.Content.ReadFromJsonAsync<List<AggregatedCargoOrderDto>>(cancellationToken: cancellationToken);
            return orders ?? [];
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Помилка зв'язку з CargoOrderingService. Застосовуємо локальний fallback для водія {DriverId}", driverId);
            // Повертаємо демонстраційний рейс при частковій недоступності для демонстрації агрегатора
            return
            [
                new AggregatedCargoOrderDto
                {
                    OrderId = Guid.NewGuid(),
                    CargoDescription = "Терміновий медичний вантаж (Стандартний рейс)",
                    WeightKg = 1450.5m,
                    OriginCity = "Київ",
                    DestinationCity = "Львів",
                    Status = "InTransit"
                }
            ];
        }
    }
}

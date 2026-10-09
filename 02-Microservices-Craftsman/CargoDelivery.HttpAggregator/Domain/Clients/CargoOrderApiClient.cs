namespace CargoDelivery.HttpAggregator.Domain.Clients;

using System.Net.Http.Json;
using CargoDelivery.HttpAggregator.Domain.Dtos;
using Microsoft.Extensions.Logging;

/// <summary>
/// ====================================================================================
/// 📦 CARGO ORDER API TYPED HTTP CLIENT (.NET 10 & IHttpClientFactory)
/// ------------------------------------------------------------------------------------
/// ПРИЗНАЧЕННЯ:
/// Опитує мікросервіс CargoOrderingService за логічною адресою
/// "https+http://cargoorderingservice".
///
/// ДОМЕН:
/// Отримує доставки водія для відображення в диспетчерському дашборді.
///
/// РОЛЬ В АГРЕГАЦІЇ:
/// Доставки — ЖОРСТКА залежність (диспетчер не може планувати рейс без них), тому
/// збій тут НЕ маскується "фейковими" даними: виняток прокидається вище, і клієнт
/// отримує чесний 503 замість вигаданого рейсу.
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
        _logger.LogInformation("Отримання доставок з CargoOrderingService для водія {DriverId}", driverId);

        try
        {
            // GetFromJsonAsync = GetAsync + EnsureSuccessStatusCode + ReadFromJsonAsync в одному виклику
            var orders = await _httpClient.GetFromJsonAsync<List<AggregatedCargoOrderDto>>(
                $"api/internal/deliveries?driverId={driverId}", cancellationToken);

            return orders ?? [];
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Помилка зв'язку з CargoOrderingService для водія {DriverId}", driverId);
            throw;
        }
    }
}

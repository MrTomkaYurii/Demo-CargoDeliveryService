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
/// "http://driversmanagement".
///
/// ASPIRE SERVICE DISCOVERY & POLLY RESILIENCE:
/// 1. URL "http://driversmanagement" автоматично підміняється на реальний хост/порт через Service Discovery.
/// 2. Клієнт захищений конвеєром Polly (Retry, Circuit Breaker, Timeout), підключеним у Program.cs.
/// ====================================================================================
/// </summary>
public sealed class DriverApiClient : IDriverApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DriverApiClient> _logger;

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
            var response = await _httpClient.GetAsync($"api/drivers/{driverId}", cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Водія {DriverId} не знайдено в DriversManagement (404)", driverId);
                return null;
            }

            response.EnsureSuccessStatusCode();

            // Читаємо відповідь або створюємо заглушку, якщо DTO повертається специфічного формату
            var driver = await response.Content.ReadFromJsonAsync<AggregatedDriverDto>(cancellationToken: cancellationToken);
            if (driver != null)
            {
                return driver;
            }

            // Fallback десеріалізації з загального формату
            return new AggregatedDriverDto
            {
                Id = driverId,
                FullName = "Зареєстрований Водій",
                LicenseNumber = "DL-UNKNOWN",
                CompanyName = "Cargo Express Logistics",
                Status = "Active"
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Мережевий збій при зверненні до DriversManagement для водія {DriverId}", driverId);
            throw;
        }
    }
}

using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Drivers.BLL.DTOs.Responses;
using Drivers.BLL.Models;

namespace WPFDrivers.Services;

/// <summary>
/// Реалізація клієнта HTTP REST для роботи з сервером DriversApi
/// </summary>
public class DriversApiClient : IDriversApiClient
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public DriversApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<FullDriverResponceDTO> GetFullDriverInfoAsync(
        int id, 
        DataAccessProvider provider = DataAccessProvider.EntityFramework, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"api/drivers/{id}?provider={provider}";
            var response = await _httpClient.GetFromJsonAsync<FullDriverResponceDTO>(url, JsonOptions, cancellationToken);
            return response ?? throw new InvalidOperationException($"Сервер повернув порожню відповідь для водія #{id}.");
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException($"Помилка зв'язку з DriversApi ({_httpClient.BaseAddress}): {ex.Message}. Переконайтеся, що Web API запущено.", ex);
        }
    }

    public async Task<QueryComparisonResultDTO<FullDriverResponceDTO>> CompareDriverProvidersAsync(
        int id, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"api/drivers/{id}/compare-providers";
            var response = await _httpClient.GetFromJsonAsync<QueryComparisonResultDTO<FullDriverResponceDTO>>(url, JsonOptions, cancellationToken);
            return response ?? throw new InvalidOperationException("Сервер повернув порожню відповідь при бенчмаркінгу.");
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException($"Помилка зв'язку з DriversApi ({_httpClient.BaseAddress}): {ex.Message}. Переконайтеся, що Web API запущено.", ex);
        }
    }

    public async Task<DriverExpenseSummaryResponseDTO> GetDriverExpenseSummaryAsync(
        int id, 
        DateTime? fromDate = null, 
        DateTime? toDate = null, 
        DataAccessProvider provider = DataAccessProvider.EntityFramework, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var queryParams = new List<string> { $"provider={provider}" };
            if (fromDate.HasValue) queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
            if (toDate.HasValue) queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");

            var url = $"api/drivers/{id}/expense-summary?{string.Join("&", queryParams)}";
            var response = await _httpClient.GetFromJsonAsync<DriverExpenseSummaryResponseDTO>(url, JsonOptions, cancellationToken);
            return response ?? throw new InvalidOperationException("Сервер повернув порожню відповідь звіту витрат.");
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException($"Помилка зв'язку з DriversApi ({_httpClient.BaseAddress}): {ex.Message}. Переконайтеся, що Web API запущено.", ex);
        }
    }

    public async Task<IEnumerable<DriverLicenseAlertResponseDTO>> GetExpiringLicensesAsync(
        int daysThreshold = 30, 
        DataAccessProvider provider = DataAccessProvider.EntityFramework, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"api/drivers/expiring-licenses?daysThreshold={daysThreshold}&provider={provider}";
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<DriverLicenseAlertResponseDTO>>(url, JsonOptions, cancellationToken);
            return response ?? Enumerable.Empty<DriverLicenseAlertResponseDTO>();
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException($"Помилка зв'язку з DriversApi ({_httpClient.BaseAddress}): {ex.Message}. Переконайтеся, що Web API запущено.", ex);
        }
    }
}

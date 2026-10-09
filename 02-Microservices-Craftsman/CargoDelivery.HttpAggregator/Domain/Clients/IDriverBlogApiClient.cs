namespace CargoDelivery.HttpAggregator.Domain.Clients;

using CargoDelivery.HttpAggregator.Domain.Dtos;

/// <summary>
/// Інтерфейс типізованого HTTP-клієнта до мікросервісу DriversBlogManagement.
/// Абстракція дозволяє підміняти клієнт у юніт-тестах (NSubstitute) без мережі.
/// </summary>
public interface IDriverBlogApiClient
{
    Task<List<AggregatedBlogNoteDto>> GetRoadNotesByDriverIdAsync(Guid driverId, CancellationToken cancellationToken = default);
}

namespace CargoDelivery.HttpAggregator.Domain.Clients;

using CargoDelivery.HttpAggregator.Domain.Dtos;

/// <summary>
/// Інтерфейс типізованого HTTP-клієнта до мікросервісу DriversBlogManagement.
/// </summary>
public interface IDriverBlogApiClient
{
    Task<List<AggregatedBlogNoteDto>> GetRoadNotesByDriverIdAsync(Guid driverId, CancellationToken cancellationToken = default);
    Task<string> ExecuteResilienceProbeAsync(string mode, CancellationToken cancellationToken = default);
}

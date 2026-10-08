namespace CargoDelivery.HttpAggregator.Tests.UnitTests;

using CargoDelivery.HttpAggregator.Domain.Clients;
using CargoDelivery.HttpAggregator.Domain.Dtos;
using CargoDelivery.HttpAggregator.Domain.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

public sealed class ShipmentAggregatorServiceTests
{
    private readonly IDriverApiClient _driverApiClient = Substitute.For<IDriverApiClient>();
    private readonly ICargoOrderApiClient _cargoOrderApiClient = Substitute.For<ICargoOrderApiClient>();
    private readonly IDriverBlogApiClient _driverBlogApiClient = Substitute.For<IDriverBlogApiClient>();
    private readonly ILogger<ShipmentAggregatorService> _logger = Substitute.For<ILogger<ShipmentAggregatorService>>();

    private readonly ShipmentAggregatorService _sut;

    public ShipmentAggregatorServiceTests()
    {
        _sut = new ShipmentAggregatorService(
            _driverApiClient,
            _cargoOrderApiClient,
            _driverBlogApiClient,
            _logger);
    }

    [Fact]
    public async Task GetShipmentSummary_WhenAllServicesRespondSuccessfully_ReturnsCompleteSummary()
    {
        // Arrange
        var driverId = Guid.NewGuid();
        var driverDto = new AggregatedDriverDto
        {
            Id = driverId,
            FullName = "Іван Коваленко",
            LicenseNumber = "DL-994421",
            CompanyName = "Експрес Логістик",
            Status = "Active"
        };
        var orders = new List<AggregatedCargoOrderDto>
        {
            new() { OrderId = Guid.NewGuid(), CargoDescription = "Електроніка", WeightKg = 500m, OriginCity = "Київ", DestinationCity = "Одеса", Status = "InTransit" }
        };
        var roadNotes = new List<AggregatedBlogNoteDto>
        {
            new() { PostId = Guid.NewGuid(), Title = "Траса М-05", Summary = "Ремонт мосту біля Умані, затримка 15 хв", CreatedAtUtc = DateTime.UtcNow }
        };

        _driverApiClient.GetDriverByIdAsync(driverId, Arg.Any<CancellationToken>()).Returns(driverDto);
        _cargoOrderApiClient.GetActiveOrdersByDriverIdAsync(driverId, Arg.Any<CancellationToken>()).Returns(orders);
        _driverBlogApiClient.GetRoadNotesByDriverIdAsync(driverId, Arg.Any<CancellationToken>()).Returns(roadNotes);

        // Act
        var result = await _sut.GetShipmentSummaryAsync(driverId);

        // Assert
        result.Should().NotBeNull();
        result.DriverId.Should().Be(driverId);
        result.Driver.FullName.Should().Be("Іван Коваленко");
        result.ActiveOrders.Should().HaveCount(1);
        result.RoadNotes.Should().HaveCount(1);
        result.IsDegraded.Should().BeFalse();
        result.AggregationStatus.Should().Be("Complete");
    }

    [Fact]
    public async Task GetShipmentSummary_WhenBlogServiceThrowsHttpRequestException_AppliesGracefulDegradation()
    {
        // Arrange
        var driverId = Guid.NewGuid();
        var driverDto = new AggregatedDriverDto
        {
            Id = driverId,
            FullName = "Петро Сидоренко",
            LicenseNumber = "DL-112233",
            CompanyName = "ТрансКарго",
            Status = "Active"
        };
        var orders = new List<AggregatedCargoOrderDto>
        {
            new() { OrderId = Guid.NewGuid(), CargoDescription = "Будівельні матеріали", WeightKg = 3200m, OriginCity = "Дніпро", DestinationCity = "Харків", Status = "Loading" }
        };

        _driverApiClient.GetDriverByIdAsync(driverId, Arg.Any<CancellationToken>()).Returns(driverDto);
        _cargoOrderApiClient.GetActiveOrdersByDriverIdAsync(driverId, Arg.Any<CancellationToken>()).Returns(orders);
        // Імітуємо відмову мікросервісу блогу (503 Service Unavailable або SocketException)
        _driverBlogApiClient.GetRoadNotesByDriverIdAsync(driverId, Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Підключення до DriversBlogManagement розірвано."));

        // Act
        var result = await _sut.GetShipmentSummaryAsync(driverId);

        // Assert: Запит не падає з кодом 500! Дані водія і замовлень на місці, IsDegraded == true
        result.Should().NotBeNull();
        result.Driver.FullName.Should().Be("Петро Сидоренко");
        result.ActiveOrders.Should().HaveCount(1);
        result.RoadNotes.Should().BeEmpty();
        result.IsDegraded.Should().BeTrue();
        result.AggregationStatus.Should().Be("Partial (Degraded)");
    }

    [Fact]
    public async Task GetShipmentSummary_WhenDriverNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var driverId = Guid.NewGuid();
        _driverApiClient.GetDriverByIdAsync(driverId, Arg.Any<CancellationToken>()).Returns((AggregatedDriverDto?)null);

        // Act
        Func<Task> act = async () => await _sut.GetShipmentSummaryAsync(driverId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{driverId}*");

        // Перевіряємо, що запити до замовлень та блогу навіть не ініціалізувалися (економія ресурсів)
        await _cargoOrderApiClient.DidNotReceive().GetActiveOrdersByDriverIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _driverBlogApiClient.DidNotReceive().GetRoadNotesByDriverIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}

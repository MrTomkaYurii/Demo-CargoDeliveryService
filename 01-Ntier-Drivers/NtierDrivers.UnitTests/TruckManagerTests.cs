using AutoMapper;
using Drivers.BLL.Managers;
using Drivers.BLL.Mapping;
using Drivers.BLL.Models;
using Drivers.DAL.ADO.Repositories.Contracts;
using Drivers.DAL.ADO.UOW.Contracts;
using Drivers.DAL.EF.Repositories.Contracts;
using Drivers.DAL.EF.UOW.Contracts;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ADOHelp = Drivers.DAL.ADO.Entities.HelpModels;
using EFHelp = Drivers.DAL.EF.Entities.HelpModels;

namespace NtierDrivers.UnitTests;

/// <summary>
/// Модульні тести для TruckManager (автопарк та аналітика обслуговування)
/// </summary>
public class TruckManagerTests
{
    private readonly Mock<ILogger<TruckManager>> _loggerMock = new();
    private readonly Mock<IUnitOfWork> _adoUowMock = new();
    private readonly Mock<IEFUnitOfWork> _efUowMock = new();
    private readonly Mock<ITruckRepository> _adoTruckRepoMock = new();
    private readonly Mock<IEFTruckRepository> _efTruckRepoMock = new();
    private readonly IMapper _mapper;

    public TruckManagerTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<BLLMappingProfile>());
        _mapper = config.CreateMapper();

        _adoUowMock.Setup(u => u.TruckRepository).Returns(_adoTruckRepoMock.Object);
        _efUowMock.Setup(u => u.EFPTruckRepository).Returns(_efTruckRepoMock.Object);
    }

    [Fact]
    public async Task GetTruckMaintenanceSummaryAsync_ReturnsMappedAnalytics()
    {
        // Arrange
        const int truckId = 7;
        var efSummary = new EFHelp.TruckMaintenanceSummary
        {
            TruckId = truckId,
            TruckNumber = "BC7777AA",
            Model = "MAN TGX",
            TotalMaintenanceCost = 12000m,
            TotalRepairsCost = 5000m,
            TotalInspectionsCount = 3,
            PassedInspectionsCount = 3,
            FailedInspectionsCount = 0
        };

        _efTruckRepoMock.Setup(r => r.GetTruckMaintenanceSummaryAsync(truckId))
            .ReturnsAsync(efSummary);

        var manager = new TruckManager(_loggerMock.Object, _mapper, _adoUowMock.Object, _efUowMock.Object);

        // Act
        var result = await manager.GetTruckMaintenanceSummaryAsync(truckId, DataAccessProvider.EntityFramework, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.TruckId.Should().Be(truckId);
        result.TruckNumber.Should().Be("BC7777AA");
        result.TotalMaintenanceCost.Should().Be(12000m);
        result.TotalRepairsCost.Should().Be(5000m);
        result.OverallServiceCost.Should().Be(17000m);
        result.PassedInspectionsCount.Should().Be(3);
    }

    [Fact]
    public async Task CompareTruckMaintenancePerformanceAsync_BenchmarksBothProviders()
    {
        // Arrange
        const int truckId = 12;
        var efSummary = new EFHelp.TruckMaintenanceSummary
        {
            TruckId = truckId,
            TruckNumber = "AI9900XX",
            Model = "Scania R450",
            TotalMaintenanceCost = 8000m,
            TotalRepairsCost = 3000m,
            TotalInspectionsCount = 2,
            PassedInspectionsCount = 2,
            FailedInspectionsCount = 0
        };

        var adoSummary = new ADOHelp.TruckMaintenanceSummary
        {
            TruckId = truckId,
            TruckNumber = "AI9900XX",
            Model = "Scania R450",
            TotalMaintenanceCost = 8000m,
            TotalRepairsCost = 3000m,
            TotalInspectionsCount = 2,
            PassedInspectionsCount = 2,
            FailedInspectionsCount = 0
        };

        _efTruckRepoMock.Setup(r => r.GetTruckMaintenanceSummaryAsync(truckId)).ReturnsAsync(efSummary);
        _adoTruckRepoMock.Setup(r => r.GetTruckMaintenanceSummaryAsync(truckId)).ReturnsAsync(adoSummary);

        var manager = new TruckManager(_loggerMock.Object, _mapper, _adoUowMock.Object, _efUowMock.Object);

        // Act
        var comparison = await manager.CompareTruckMaintenancePerformanceAsync(truckId, CancellationToken.None);

        // Assert
        comparison.Should().NotBeNull();
        comparison.QueryName.Should().Be("GetTruckMaintenanceSummary");
        comparison.DataConsistent.Should().BeTrue();
        comparison.FasterProvider.Should().BeOneOf("Dapper", "Entity Framework Core");
    }
}

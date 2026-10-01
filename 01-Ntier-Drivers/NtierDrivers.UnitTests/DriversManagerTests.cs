using AutoMapper;
using Drivers.BLL.Exceptions;
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
using ADOEntities = Drivers.DAL.ADO.Entities;
using ADOHelp = Drivers.DAL.ADO.Entities.HelpModels;
using EFEntities = Drivers.DAL.EF.Entities;
using EFHelp = Drivers.DAL.EF.Entities.HelpModels;

namespace NtierDrivers.UnitTests;

/// <summary>
/// Модульні тести бізнес-менеджера DriversManager з перевіркою обох провайдерів та бенчмаркінгу
/// </summary>
public class DriversManagerTests
{
    private readonly Mock<ILogger<DriversManager>> _loggerMock = new();
    private readonly Mock<IUnitOfWork> _adoUowMock = new();
    private readonly Mock<IEFUnitOfWork> _efUowMock = new();
    private readonly Mock<IDriverRepository> _adoDriverRepoMock = new();
    private readonly Mock<IEFDriverRepository> _efDriverRepoMock = new();
    private readonly IMapper _mapper;

    public DriversManagerTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<BLLMappingProfile>());
        _mapper = config.CreateMapper();

        _adoUowMock.Setup(u => u.DriverRepository).Returns(_adoDriverRepoMock.Object);
        _efUowMock.Setup(u => u.EFDriverRepository).Returns(_efDriverRepoMock.Object);
    }

    [Fact]
    public async Task GetFullInfoAboutDriver_WhenProviderIsEntityFramework_CallsEFRepository()
    {
        // Arrange
        const int driverId = 1;
        var efDriver = new EFEntities.EFDriver
        {
            Id = driverId,
            FirstName = "Тарас",
            LastName = "Шевченко",
            Birthdate = new DateTime(1980, 3, 9),
            DriverLicenseNumber = "AA111222"
        };

        _efDriverRepoMock.Setup(r => r.GetCompleteEntityAsync(driverId))
            .ReturnsAsync(efDriver);

        var manager = new DriversManager(_loggerMock.Object, _mapper, _adoUowMock.Object, _efUowMock.Object);

        // Act
        var result = await manager.GetFullInfoAboutDriver(driverId, DataAccessProvider.EntityFramework, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(driverId);
        result.LastName.Should().Be("Шевченко");

        _efDriverRepoMock.Verify(r => r.GetCompleteEntityAsync(driverId), Times.Once);
        _adoDriverRepoMock.Verify(r => r.GetCompleteEntityAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetFullInfoAboutDriver_WhenProviderIsDapper_CallsADORepository()
    {
        // Arrange
        const int driverId = 2;
        var adoDriver = new ADOEntities.Driver
        {
            Id = driverId,
            FirstName = "Богдан",
            LastName = "Хмельницький",
            Birthdate = new DateTime(1985, 4, 12),
            DriverLicenseNumber = "BB333444"
        };

        _adoDriverRepoMock.Setup(r => r.GetCompleteEntityAsync(driverId))
            .ReturnsAsync(adoDriver);

        var manager = new DriversManager(_loggerMock.Object, _mapper, _adoUowMock.Object, _efUowMock.Object);

        // Act
        var result = await manager.GetFullInfoAboutDriver(driverId, DataAccessProvider.Dapper, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(driverId);
        result.LastName.Should().Be("Хмельницький");

        _adoDriverRepoMock.Verify(r => r.GetCompleteEntityAsync(driverId), Times.Once);
        _efDriverRepoMock.Verify(r => r.GetCompleteEntityAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetFullInfoAboutDriver_WhenNotFound_ThrowsNotFoundException()
    {
        // Arrange
        const int missingId = 999;
        _efDriverRepoMock.Setup(r => r.GetCompleteEntityAsync(missingId))
            .ReturnsAsync((EFEntities.EFDriver?)null);

        var manager = new DriversManager(_loggerMock.Object, _mapper, _adoUowMock.Object, _efUowMock.Object);

        // Act & Assert
        var act = async () => await manager.GetFullInfoAboutDriver(missingId, DataAccessProvider.EntityFramework, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CompareDriverQueryPerformanceAsync_ExecutesBothProviders_AndReturnsBenchmark()
    {
        // Arrange
        const int driverId = 10;
        var efDriver = new EFEntities.EFDriver
        {
            Id = driverId,
            FirstName = "Іван",
            LastName = "Франко",
            Birthdate = new DateTime(1975, 8, 27),
            DriverLicenseNumber = "IF123456",
            Expenses = new List<EFEntities.EFExpense>()
        };

        var adoDriver = new ADOEntities.Driver
        {
            Id = driverId,
            FirstName = "Іван",
            LastName = "Франко",
            Birthdate = new DateTime(1975, 8, 27),
            DriverLicenseNumber = "IF123456",
            Expenses = new List<ADOEntities.Expenses>()
        };

        _efDriverRepoMock.Setup(r => r.GetCompleteEntityAsync(driverId)).ReturnsAsync(efDriver);
        _adoDriverRepoMock.Setup(r => r.GetCompleteEntityAsync(driverId)).ReturnsAsync(adoDriver);

        var manager = new DriversManager(_loggerMock.Object, _mapper, _adoUowMock.Object, _efUowMock.Object);

        // Act
        var benchmark = await manager.CompareDriverQueryPerformanceAsync(driverId, CancellationToken.None);

        // Assert
        benchmark.Should().NotBeNull();
        benchmark.QueryName.Should().Be("GetCompleteDriverEntity");
        benchmark.DataConsistent.Should().BeTrue();
        benchmark.FasterProvider.Should().BeOneOf("Dapper", "Entity Framework Core");
        benchmark.SpeedupFactor.Should().BeGreaterThanOrEqualTo(1.0);

        _efDriverRepoMock.Verify(r => r.GetCompleteEntityAsync(driverId), Times.Once);
        _adoDriverRepoMock.Verify(r => r.GetCompleteEntityAsync(driverId), Times.Once);
    }

    [Fact]
    public async Task CompareExpenseSummaryPerformanceAsync_CalculatesConsistencyCorrectly()
    {
        // Arrange
        const int driverId = 5;
        var efSummary = new EFHelp.DriverExpenseSummary
        {
            DriverId = driverId,
            ExpenseCount = 3,
            TotalDriverPayment = 6000m,
            TotalFuelCost = 4000m,
            TotalMaintenanceCost = 1000m,
            GrandTotal = 11000m,
            AverageExpense = 3666.67m
        };

        var adoSummary = new ADOHelp.DriverExpenseSummary
        {
            DriverId = driverId,
            ExpenseCount = 3,
            TotalDriverPayment = 6000m,
            TotalFuelCost = 4000m,
            TotalMaintenanceCost = 1000m,
            GrandTotal = 11000m,
            AverageExpense = 3666.67m
        };

        _efDriverRepoMock.Setup(r => r.GetDriverExpenseSummaryAsync(driverId, null, null))
            .ReturnsAsync(efSummary);
        _adoDriverRepoMock.Setup(r => r.GetDriverExpenseSummaryAsync(driverId, null, null))
            .ReturnsAsync(adoSummary);

        var manager = new DriversManager(_loggerMock.Object, _mapper, _adoUowMock.Object, _efUowMock.Object);

        // Act
        var result = await manager.CompareExpenseSummaryPerformanceAsync(driverId, null, null, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.DataConsistent.Should().BeTrue();
        result.Result!.GrandTotal.Should().Be(11000m);
        result.Result.ExpenseCount.Should().Be(3);
    }
}

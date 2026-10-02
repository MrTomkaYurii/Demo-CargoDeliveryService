using AutoMapper;
using Drivers.BLL.DTOs.Requests;
using Drivers.BLL.Managers;
using Drivers.BLL.Mapping;
using Drivers.BLL.Models;
using Drivers.DAL.ADO.UOW.Contracts;
using Drivers.DAL.EF.Entities;
using Drivers.DAL.EF.Entities.HelpModels;
using Drivers.DAL.EF.Repositories.Contracts;
using Drivers.DAL.EF.UOW.Contracts;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace NtierDrivers.UnitTests;

/// <summary>
/// Тести перевірки наскрізної передачі CancellationToken крізь бізнес-шар BLL та репозиторії DAL EF Core.
/// Демонструє реакцію системи на скасування асинхронних операцій (User Cancel, Request Timeout, Client Disconnect).
/// </summary>
public class CancellationTokenTests
{
    private readonly Mock<ILogger<DriversManager>> _loggerMock = new();
    private readonly Mock<IUnitOfWork> _adoUowMock = new();
    private readonly Mock<IEFUnitOfWork> _efUowMock = new();
    private readonly Mock<IEFDriverRepository> _efDriverRepoMock = new();
    private readonly IMapper _mapper;

    public CancellationTokenTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<BLLMappingProfile>());
        _mapper = config.CreateMapper();

        _efUowMock.Setup(u => u.EFDriverRepository).Returns(_efDriverRepoMock.Object);
    }

    [Fact]
    public async Task GetFullInfoAboutDriver_WhenCancellationTokenCanceled_ThrowsOperationCanceledException()
    {
        // Arrange: Створюємо токен, який вже перебуває у стані скасування (імітація скасування користувачем)
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var cancellationToken = cts.Token;
        const int driverId = 42;

        _efDriverRepoMock
            .Setup(r => r.GetCompleteEntityAsync(driverId, cancellationToken))
            .ThrowsAsync(new OperationCanceledException(cancellationToken));

        var manager = new DriversManager(_loggerMock.Object, _mapper, _adoUowMock.Object, _efUowMock.Object);

        // Act
        var act = async () => await manager.GetFullInfoAboutDriver(driverId, DataAccessProvider.EntityFramework, cancellationToken);

        // Assert: BLL не повинен маскувати OperationCanceledException або перетворювати його на іншу помилку
        await act.Should().ThrowAsync<OperationCanceledException>();
        _efDriverRepoMock.Verify(r => r.GetCompleteEntityAsync(driverId, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetListOfAllDrivers_WhenCancellationTokenCanceled_PropagatesTokenToRepository()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var cancellationToken = cts.Token;

        _efDriverRepoMock
            .Setup(r => r.GetAllAsync(cancellationToken))
            .ThrowsAsync(new OperationCanceledException(cancellationToken));

        var manager = new DriversManager(_loggerMock.Object, _mapper, _adoUowMock.Object, _efUowMock.Object);

        // Act
        var act = async () => await manager.GetListOfAllDrivers(DataAccessProvider.EntityFramework, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
        _efDriverRepoMock.Verify(r => r.GetAllAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetPaginatedDrivers_WhenCancellationTokenCanceled_PropagatesTokenToRepository()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var cancellationToken = cts.Token;
        var parameters = new DriverParameters { PageNumber = 1, PageSize = 10 };

        _efDriverRepoMock
            .Setup(r => r.GetPaginatedDriversAsync(parameters, cancellationToken))
            .ThrowsAsync(new OperationCanceledException(cancellationToken));

        var manager = new DriversManager(_loggerMock.Object, _mapper, _adoUowMock.Object, _efUowMock.Object);

        // Act
        var act = async () => await manager.GetPaginatedDrivers(parameters, DataAccessProvider.EntityFramework, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
        _efDriverRepoMock.Verify(r => r.GetPaginatedDriversAsync(parameters, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task CompareDriverQueryPerformanceAsync_WhenCancellationTokenCanceled_AbortsExecution()
    {
        // Arrange: Імітуємо скасування бенчмарку під час виконання першого EF запиту
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var cancellationToken = cts.Token;
        const int driverId = 15;

        _efDriverRepoMock
            .Setup(r => r.GetCompleteEntityAsync(driverId, cancellationToken))
            .ThrowsAsync(new OperationCanceledException(cancellationToken));

        var manager = new DriversManager(_loggerMock.Object, _mapper, _adoUowMock.Object, _efUowMock.Object);

        // Act
        var act = async () => await manager.CompareDriverQueryPerformanceAsync(driverId, cancellationToken);

        // Assert: Бенчмаркінг негайно зупиняється без запуску Dapper чи підрахунку часу
        await act.Should().ThrowAsync<OperationCanceledException>();
        _efDriverRepoMock.Verify(r => r.GetCompleteEntityAsync(driverId, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task AddDriverToSystemAsync_WhenCancelled_DoesNotCommitUnitOfWork()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var cancellationToken = cts.Token;

        var driverDto = new MiniDriverReqDTO
        {
            FirstName = "Олексій",
            LastName = "Коваленко",
            Birthdate = new DateTime(1990, 5, 12),
            DriverLicenseNumber = "BX987654"
        };

        _efDriverRepoMock
            .Setup(r => r.AddAsync(It.IsAny<EFDriver>(), cancellationToken))
            .ThrowsAsync(new OperationCanceledException(cancellationToken));

        var manager = new DriversManager(_loggerMock.Object, _mapper, _adoUowMock.Object, _efUowMock.Object);

        // Act
        var act = async () => await manager.AddDriverToSystemAsync(driverDto, DataAccessProvider.EntityFramework, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
        _efUowMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

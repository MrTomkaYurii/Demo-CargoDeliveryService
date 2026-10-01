using Drivers.Api.Controllers;
using Drivers.BLL.DTOs.Requests;
using Drivers.BLL.DTOs.Responses;
using Drivers.BLL.Managers.Contracts;
using Drivers.BLL.Models;
using Drivers.DAL.EF.Entities.HelpModels;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace NtierDrivers.UnitTests;

/// <summary>
/// Модульні тести для DriversController (Web API)
/// </summary>
public class DriversControllerTests
{
    private readonly Mock<ILogger<DriversController>> _loggerMock = new();
    private readonly Mock<IDriversManager> _managerMock = new();
    private readonly DriversController _controller;

    public DriversControllerTests()
    {
        _controller = new DriversController(_loggerMock.Object, _managerMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Fact]
    public async Task GetFullInfoAboutDriver_ReturnsOkResult_WithDriverData()
    {
        // Arrange
        const int driverId = 3;
        var expectedDto = new FullDriverResponceDTO
        {
            Id = driverId,
            FirstName = "Леся",
            LastName = "Українка"
        };

        _managerMock.Setup(m => m.GetFullInfoAboutDriver(driverId, DataAccessProvider.EntityFramework, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        // Act
        var actionResult = await _controller.GetFullInfoAboutDriver(driverId, DataAccessProvider.EntityFramework);

        // Assert
        var okResult = actionResult.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().BeEquivalentTo(expectedDto);
    }

    [Fact]
    public async Task GetAllDriversPaginatedAsync_WhenValid_ReturnsOkAndSetsPaginationHeader()
    {
        // Arrange
        var parameters = new DriverParameters { PageNumber = 1, PageSize = 10 };
        var pagedResponse = new PagedResponseDTO<ShortDriverResponceDTO>
        {
            Items = new List<ShortDriverResponceDTO>
            {
                new() { Id = 1, FirstName = "Григорій", LastName = "Сковорода" }
            },
            CurrentPage = 1,
            PageSize = 10,
            TotalCount = 1,
            TotalPages = 1
        };

        _managerMock.Setup(m => m.GetPaginatedDrivers(parameters, DataAccessProvider.EntityFramework, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResponse);

        // Act
        var result = await _controller.GetAllDriversPaginatedAsync(parameters, DataAccessProvider.EntityFramework);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        _controller.Response.Headers.ContainsKey("X-Pagination").Should().BeTrue();
    }

    [Fact]
    public async Task GetDriverExpenseSummary_WhenFromDateIsAfterToDate_ReturnsBadRequest()
    {
        // Arrange
        var fromDate = new DateTime(2026, 12, 31);
        var toDate = new DateTime(2026, 1, 1);

        // Act
        var result = await _controller.GetDriverExpenseSummary(1, fromDate, toDate);

        // Assert
        var badRequest = result.Result as BadRequestObjectResult;
        badRequest.Should().NotBeNull();
        badRequest!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task CompareProviders_ReturnsOk_WithComparisonData()
    {
        // Arrange
        const int driverId = 5;
        var comparisonResult = new QueryComparisonResultDTO<FullDriverResponceDTO>
        {
            QueryName = "GetCompleteDriverEntity",
            EntityFrameworkExecutionTimeMs = 15.4,
            DapperExecutionTimeMs = 4.2,
            FasterProvider = "Dapper",
            SpeedupFactor = 3.67,
            DataConsistent = true
        };

        _managerMock.Setup(m => m.CompareDriverQueryPerformanceAsync(driverId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comparisonResult);

        // Act
        var result = await _controller.CompareProviders(driverId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().BeEquivalentTo(comparisonResult);
    }

    [Fact]
    public async Task AssessAndDispatchDriverForTripAsync_ReturnsOk_WithAssessmentData()
    {
        // Arrange
        var request = new DriverTripDispatchRequestDTO
        {
            DriverId = 1,
            TruckId = 1,
            CargoType = "Будматеріали",
            DistanceKm = 500,
            BaseRatePerKm = 10,
            EstimatedDurationDays = 2
        };

        var expectedAssessment = new DriverTripDispatchAssessmentDTO
        {
            DriverId = 1,
            TruckId = 1,
            DriverFullName = "Тарас Шевченко",
            TruckInfo = "Volvo FH",
            IsDispatchAllowed = true,
            DecisionStatus = "Approved",
            TotalTripBudget = 15000m
        };

        _managerMock.Setup(m => m.AssessAndDispatchDriverForTripAsync(request, DataAccessProvider.EntityFramework, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAssessment);

        // Act
        var result = await _controller.AssessAndDispatchDriverForTripAsync(request, DataAccessProvider.EntityFramework);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().BeEquivalentTo(expectedAssessment);
    }

    [Fact]
    public async Task ConductDriverAnnualPerformanceAuditAsync_ReturnsOk_WithAuditReport()
    {
        // Arrange
        const int driverId = 1;
        const int year = 2025;
        var expectedReport = new DriverAnnualAuditReportDTO
        {
            DriverId = driverId,
            AuditYear = year,
            DriverFullName = "Тарас Шевченко",
            TotalTripsCompleted = 5,
            AssignedDriverGrade = "Grade A (Platinum)",
            OverallPerformanceRating = 92.5,
            CalculatedAnnualBonus = 12000m
        };

        _managerMock.Setup(m => m.ConductDriverAnnualPerformanceAuditAsync(driverId, year, DataAccessProvider.EntityFramework, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedReport);

        // Act
        var result = await _controller.ConductDriverAnnualPerformanceAuditAsync(driverId, year, DataAccessProvider.EntityFramework);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().BeEquivalentTo(expectedReport);
    }

    [Fact]
    public async Task ConductDriverAnnualPerformanceAuditAsync_WhenInvalidYear_ReturnsBadRequest()
    {
        // Arrange
        const int driverId = 1;
        const int invalidYear = 1990;

        // Act
        var result = await _controller.ConductDriverAnnualPerformanceAuditAsync(driverId, invalidYear);

        // Assert
        var badRequest = result.Result as BadRequestObjectResult;
        badRequest.Should().NotBeNull();
        badRequest!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }
}

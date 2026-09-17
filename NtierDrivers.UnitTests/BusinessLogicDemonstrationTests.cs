using AutoMapper;
using Drivers.BLL.DTOs.Requests;
using Drivers.BLL.DTOs.Responses;
using Drivers.BLL.Exceptions;
using Drivers.BLL.Managers;
using Drivers.BLL.Mapping;
using Drivers.BLL.Models;
using Drivers.DAL.ADO.Repositories.Contracts;
using Drivers.DAL.ADO.UOW.Contracts;
using Drivers.DAL.EF.Entities;
using Drivers.DAL.EF.Repositories.Contracts;
using Drivers.DAL.EF.UOW.Contracts;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ADOEntities = Drivers.DAL.ADO.Entities;
using EFEntities = Drivers.DAL.EF.Entities;

namespace NtierDrivers.UnitTests;

/// <summary>
/// Модульні тести для демонстрації двох методів бізнес-логіки (BLL):
/// 1. AssessAndDispatchDriverForTripAsync (Комплаєнс, перевірка автопарку, ціноутворення та збереження бюджету)
/// 2. ConductDriverAnnualPerformanceAuditAsync (KPI скоринг, грейдування та нарахування річного бонусу)
/// </summary>
public class BusinessLogicDemonstrationTests
{
    private readonly Mock<ILogger<DriversManager>> _loggerMock = new();
    private readonly Mock<IUnitOfWork> _adoUowMock = new();
    private readonly Mock<IEFUnitOfWork> _efUowMock = new();

    // EF Репозиторії
    private readonly Mock<IEFDriverRepository> _efDriverRepoMock = new();
    private readonly Mock<IEFTruckRepository> _efTruckRepoMock = new();
    private readonly Mock<IEFInspectionRepository> _efInspectionRepoMock = new();
    private readonly Mock<IEFExpenseRepository> _efExpenseRepoMock = new();

    // ADO/Dapper Репозиторії
    private readonly Mock<IDriverRepository> _adoDriverRepoMock = new();
    private readonly Mock<ITruckRepository> _adoTruckRepoMock = new();
    private readonly Mock<IInspectionRepository> _adoInspectionRepoMock = new();
    private readonly Mock<IExpensesRepository> _adoExpenseRepoMock = new();

    private readonly IMapper _mapper;

    public BusinessLogicDemonstrationTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<BLLMappingProfile>());
        _mapper = config.CreateMapper();

        // Налаштування EF Unit of Work
        _efUowMock.Setup(u => u.EFDriverRepository).Returns(_efDriverRepoMock.Object);
        _efUowMock.Setup(u => u.EFPTruckRepository).Returns(_efTruckRepoMock.Object);
        _efUowMock.Setup(u => u.EFInspectionRepository).Returns(_efInspectionRepoMock.Object);
        _efUowMock.Setup(u => u.EFExpenseRepository).Returns(_efExpenseRepoMock.Object);

        // Налаштування ADO/Dapper Unit of Work
        _adoUowMock.Setup(u => u.DriverRepository).Returns(_adoDriverRepoMock.Object);
        _adoUowMock.Setup(u => u.TruckRepository).Returns(_adoTruckRepoMock.Object);
        _adoUowMock.Setup(u => u.InspectionRepository).Returns(_adoInspectionRepoMock.Object);
        _adoUowMock.Setup(u => u.ExpensesRepository).Returns(_adoExpenseRepoMock.Object);
    }

    // =========================================================================================================
    // ТЕСТИ ДЛЯ МЕТОДУ 1: AssessAndDispatchDriverForTripAsync
    // =========================================================================================================

    [Fact]
    public async Task AssessAndDispatch_WhenDriverIsUnderageOrNoCategoryC_RejectsTripWithCriticalViolations()
    {
        // Arrange: Водієві лише 19 років і категорія лише "B"
        const int driverId = 101;
        const int truckId = 11;

        var driver = new EFEntities.EFDriver
        {
            Id = driverId,
            FirstName = "Юрій",
            LastName = "Молодий",
            Birthdate = DateTime.UtcNow.AddYears(-19), // 19 років (менше 21)
            EmploymentStatus = "Active",
            DriverLicenseCategory = "B",               // Немає вантажної категорії C
            DriverLicenseExpirationDate = DateTime.UtcNow.AddYears(2),
            EmploymentStartDate = DateTime.UtcNow.AddMonths(-6)
        };

        var truck = new EFEntities.EFTruck
        {
            Id = truckId,
            TruckNumber = "AA9988BB",
            Model = "Volvo FH 540"
        };

        _efDriverRepoMock.Setup(r => r.GetByIdAsync(driverId)).ReturnsAsync(driver);
        _efTruckRepoMock.Setup(r => r.GetByIdAsync(truckId)).ReturnsAsync(truck);
        _efInspectionRepoMock.Setup(r => r.GetInspectionsByTruckIdAsync(truckId))
            .ReturnsAsync(new List<EFEntities.EFInspection>
            {
                new() { Id = 1, TruckId = truckId, Result = true, InspectionDate = DateTime.UtcNow.AddDays(-30) }
            });

        var manager = new DriversManager(_loggerMock.Object, _mapper, _adoUowMock.Object, _efUowMock.Object);

        var request = new DriverTripDispatchRequestDTO
        {
            DriverId = driverId,
            TruckId = truckId,
            CargoType = "Стандартний палетний",
            DistanceKm = 800,
            BaseRatePerKm = 12.0m,
            EstimatedDurationDays = 2
        };

        // Act
        var result = await manager.AssessAndDispatchDriverForTripAsync(request, DataAccessProvider.EntityFramework);

        // Assert
        result.Should().NotBeNull();
        result.IsDispatchAllowed.Should().BeFalse();
        result.DecisionStatus.Should().Be("Rejected");
        result.CriticalViolations.Should().HaveCountGreaterOrEqualTo(2);
        result.CriticalViolations.Should().Contain(v => v.Contains("21 р."));
        result.CriticalViolations.Should().Contain(v => v.Contains("'C' або 'CE'"));
        result.CreatedExpensePlanId.Should().BeNull();
    }

    [Fact]
    public async Task AssessAndDispatch_WhenTruckFailedInspection_RejectsTripDueToFleetSafetyPolicy()
    {
        // Arrange: Водій ідеальний, але тягач має провалений техогляд
        const int driverId = 102;
        const int truckId = 12;

        var driver = new ADOEntities.Driver
        {
            Id = driverId,
            FirstName = "Олег",
            LastName = "Досвідчений",
            Birthdate = DateTime.UtcNow.AddYears(-35),
            EmploymentStatus = "Active",
            DriverLicenseCategory = "C, CE",
            DriverLicenseExpirationDate = DateTime.UtcNow.AddYears(3),
            EmploymentStartDate = DateTime.UtcNow.AddYears(-5)
        };

        var truck = new ADOEntities.Truck
        {
            Id = truckId,
            TruckNumber = "BC4433CB",
            Model = "Scania R450"
        };

        _adoDriverRepoMock.Setup(r => r.GetByIdAsync(driverId)).ReturnsAsync(driver);
        _adoTruckRepoMock.Setup(r => r.GetAsync(truckId)).ReturnsAsync(truck);
        _adoInspectionRepoMock.Setup(r => r.GetInspectionsByTruckIdAsync(truckId))
            .ReturnsAsync(new List<ADOEntities.Inspection>
            {
                new() { Id = 1, TruckID = truckId, Result = false, InspectionDate = DateTime.UtcNow.AddDays(-5) } // ПРОВАЛЕНО
            });

        var manager = new DriversManager(_loggerMock.Object, _mapper, _adoUowMock.Object, _efUowMock.Object);

        var request = new DriverTripDispatchRequestDTO
        {
            DriverId = driverId,
            TruckId = truckId,
            CargoType = "Будівельні матеріали",
            DistanceKm = 500,
            BaseRatePerKm = 10m,
            EstimatedDurationDays = 1
        };

        // Act
        var result = await manager.AssessAndDispatchDriverForTripAsync(request, DataAccessProvider.Dapper);

        // Assert
        result.Should().NotBeNull();
        result.IsDispatchAllowed.Should().BeFalse();
        result.DecisionStatus.Should().Be("Rejected");
        result.CriticalViolations.Should().Contain(v => v.Contains("НЕ ПРОЙДЕНО"));
    }

    [Fact]
    public async Task AssessAndDispatch_WhenAllCompliancesMet_ApprovesTripCalculatesPricingAndCreatesExpensePlan()
    {
        // Arrange: Досвідчений водій (стаж 6 років -> +20%), Небезпечний вантаж ADR (+35%), 1000 км по 15 грн/км, 3 дні
        const int driverId = 103;
        const int truckId = 13;

        var driver = new EFEntities.EFDriver
        {
            Id = driverId,
            FirstName = "Андрій",
            LastName = "Професіонал",
            Birthdate = DateTime.UtcNow.AddYears(-38),
            EmploymentStatus = "Active",
            DriverLicenseCategory = "C, CE",
            DriverLicenseExpirationDate = DateTime.UtcNow.AddYears(4),
            EmploymentStartDate = DateTime.UtcNow.AddYears(-6) // 6 років стажу
        };

        var truck = new EFEntities.EFTruck
        {
            Id = truckId,
            TruckNumber = "AI1234KA",
            Model = "MAN TGX 18.500"
        };

        _efDriverRepoMock.Setup(r => r.GetByIdAsync(driverId)).ReturnsAsync(driver);
        _efTruckRepoMock.Setup(r => r.GetByIdAsync(truckId)).ReturnsAsync(truck);
        _efInspectionRepoMock.Setup(r => r.GetInspectionsByTruckIdAsync(truckId))
            .ReturnsAsync(new List<EFEntities.EFInspection>
            {
                new() { Id = 10, TruckId = truckId, Result = true, InspectionDate = DateTime.UtcNow.AddDays(-20) }
            });

        _efExpenseRepoMock.Setup(r => r.AddAsync(It.IsAny<EFExpense>()))
            .ReturnsAsync((EFExpense exp) =>
            {
                exp.Id = 777;
                return exp;
            });

        _efUowMock.Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var manager = new DriversManager(_loggerMock.Object, _mapper, _adoUowMock.Object, _efUowMock.Object);

        var request = new DriverTripDispatchRequestDTO
        {
            DriverId = driverId,
            TruckId = truckId,
            CargoType = "Небезпечний вантаж (ADR: Цистерна)",
            DistanceKm = 1000m,
            BaseRatePerKm = 15.0m,
            EstimatedDurationDays = 3
        };

        // Act
        var result = await manager.AssessAndDispatchDriverForTripAsync(request, DataAccessProvider.EntityFramework);

        // Assert
        result.Should().NotBeNull();
        result.IsDispatchAllowed.Should().BeTrue();
        result.DecisionStatus.Should().Be("Approved");
        result.CriticalViolations.Should().BeEmpty();

        // Перевірка тарифної математики (Pricing Engine):
        // Базова ставка: 1000 * 15 = 15 000
        result.BaseDriverPayment.Should().Be(15000.0m);
        // Бонус за стаж (>=5 років -> +20%): 15 000 * 0.20 = 3 000
        result.ExperienceBonus.Should().Be(3000.0m);
        // Доплата за ADR (+35%): 15 000 * 0.35 = 5 250
        result.CargoRiskBonus.Should().Be(5250.0m);
        // Добові (3 дні * 800 грн): 2 400
        result.PerDiemAllowance.Should().Be(2400.0m);
        // Сумарна виплата водієві: 15000 + 3000 + 5250 + 2400 = 25 650
        result.TotalDriverCompensation.Should().Be(25650.0m);

        // Паливо: (1000 / 100) * 32 * 54.50 = 320 * 54.50 = 17 440
        result.EstimatedFuelExpense.Should().Be(17440.0m);
        // Резерв 10%: 1 744
        result.EmergencyReserveFund.Should().Be(1744.0m);
        // Загальний кошторис: 25650 + 17440 + 1744 = 44 834
        result.TotalTripBudget.Should().Be(44834.0m);

        // Перевірка фіксації транзакції
        result.CreatedExpensePlanId.Should().Be(777);
        _efExpenseRepoMock.Verify(r => r.AddAsync(It.IsAny<EFExpense>()), Times.Once);
        _efUowMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // =========================================================================================================
    // ТЕСТИ ДЛЯ МЕТОДУ 2: ConductDriverAnnualPerformanceAuditAsync
    // =========================================================================================================

    [Fact]
    public async Task ConductAnnualAudit_WhenNoActivityInYear_ReturnsNoActivityGrade()
    {
        // Arrange
        const int driverId = 201;
        const int auditYear = 2025;

        var driver = new EFEntities.EFDriver
        {
            Id = driverId,
            FirstName = "Михайло",
            LastName = "Відсутній",
            DriverLicenseExpirationDate = DateTime.UtcNow.AddYears(2)
        };

        _efDriverRepoMock.Setup(r => r.GetByIdAsync(driverId)).ReturnsAsync(driver);
        _efExpenseRepoMock.Setup(r => r.GetExpensesByDriverIdAsync(driverId))
            .ReturnsAsync(new List<EFExpense>()); // Немає записів

        var manager = new DriversManager(_loggerMock.Object, _mapper, _adoUowMock.Object, _efUowMock.Object);

        // Act
        var report = await manager.ConductDriverAnnualPerformanceAuditAsync(driverId, auditYear, DataAccessProvider.EntityFramework);

        // Assert
        report.Should().NotBeNull();
        report.TotalTripsCompleted.Should().Be(0);
        report.AssignedDriverGrade.Should().Contain("NoActivity");
        report.CalculatedAnnualBonus.Should().Be(0m);
        report.IsBonusRecordedInExpenses.Should().BeFalse();
    }

    [Fact]
    public async Task ConductAnnualAudit_WhenHighPerformance_AssignsGradeA_AndCalculatesTenPercentBonus()
    {
        // Arrange
        const int driverId = 202;
        const int auditYear = 2025;

        var driver = new EFEntities.EFDriver
        {
            Id = driverId,
            FirstName = "Василь",
            LastName = "Еталонний",
            DriverLicenseExpirationDate = DateTime.UtcNow.AddYears(2)
        };

        // Створюємо 5 рейсів за 2025 рік з високою ефективністю:
        // Зарплата: 100 000 грн
        // Пальне: 60 000 грн
        // Ремонти: 5 000 грн
        // Загальні: 165 000 грн. Частка пального = 60000 / 165000 = 36.36% (<= 40% -> FuelScore = 95)
        // Ремонти / ЗП = 5000 / 100000 = 5% (<= 10% -> VehicleCareScore = 98)
        // Дисципліна = 90.
        // Інтеграл: 95*0.35 + 98*0.35 + 90*0.30 = 33.25 + 34.3 + 27.0 = 94.55 (>= 85 -> Grade A)
        // Премія = 100 000 * 10% = 10 000 грн.
        var expensesList = new List<EFExpense>
        {
            new() { Id = 1, DriverId = driverId, TruckId = 5, DriverPayment = 20000m, FuelCost = 12000m, MaintenanceCost = 1000m, Date = new DateTime(2025, 2, 10), Category = "Рейс 1" },
            new() { Id = 2, DriverId = driverId, TruckId = 5, DriverPayment = 20000m, FuelCost = 12000m, MaintenanceCost = 1000m, Date = new DateTime(2025, 4, 15), Category = "Рейс 2" },
            new() { Id = 3, DriverId = driverId, TruckId = 5, DriverPayment = 20000m, FuelCost = 12000m, MaintenanceCost = 1000m, Date = new DateTime(2025, 6, 20), Category = "Рейс 3" },
            new() { Id = 4, DriverId = driverId, TruckId = 5, DriverPayment = 20000m, FuelCost = 12000m, MaintenanceCost = 1000m, Date = new DateTime(2025, 8, 25), Category = "Рейс 4" },
            new() { Id = 5, DriverId = driverId, TruckId = 5, DriverPayment = 20000m, FuelCost = 12000m, MaintenanceCost = 1000m, Date = new DateTime(2025, 10, 30), Category = "Рейс 5" }
        };

        _efDriverRepoMock.Setup(r => r.GetByIdAsync(driverId)).ReturnsAsync(driver);
        _efExpenseRepoMock.Setup(r => r.GetExpensesByDriverIdAsync(driverId)).ReturnsAsync(expensesList);

        _efExpenseRepoMock.Setup(r => r.AddAsync(It.IsAny<EFExpense>()))
            .ReturnsAsync((EFExpense e) => { e.Id = 888; return e; });
        _efUowMock.Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var manager = new DriversManager(_loggerMock.Object, _mapper, _adoUowMock.Object, _efUowMock.Object);

        // Act
        var report = await manager.ConductDriverAnnualPerformanceAuditAsync(driverId, auditYear, DataAccessProvider.EntityFramework);

        // Assert
        report.Should().NotBeNull();
        report.TotalTripsCompleted.Should().Be(5);
        report.TotalEarnings.Should().Be(100000m);
        report.FuelEfficiencyScore.Should().Be(95.0);
        report.VehicleCareScore.Should().Be(98.0);
        report.SafetyComplianceScore.Should().Be(90.0);
        report.OverallPerformanceRating.Should().BeGreaterThanOrEqualTo(85.0);
        report.AssignedDriverGrade.Should().Contain("Grade A (Platinum)");
        report.CalculatedAnnualBonus.Should().Be(10000m); // 10% від 100 000
        report.IsBonusRecordedInExpenses.Should().BeTrue();

        _efExpenseRepoMock.Verify(r => r.AddAsync(It.Is<EFExpense>(e => e.DriverPayment == 10000m && e.Category == "Річна премія водія")), Times.Once);
        _efUowMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

using AutoMapper;
using Drivers.BLL.DTOs.Requests;
using Drivers.BLL.Managers;
using Drivers.BLL.Mapping;
using Drivers.BLL.Models;
using Drivers.DAL.ADO.Entities;
using Drivers.DAL.ADO.Repositories.Contracts;
using Drivers.DAL.ADO.UOW.Contracts;
using Drivers.DAL.EF.Entities;
using Drivers.DAL.EF.Repositories.Contracts;
using Drivers.DAL.EF.UOW.Contracts;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace NtierDrivers.UnitTests;

/// <summary>
/// Модульні тести для додаткових менеджерів (Company, Expense, Inspection, Repair)
/// з перевіркою коректного виклику Entity Framework Core та ADO.NET / Dapper провайдерів.
/// </summary>
public class AuxiliaryManagersTests
{
    private readonly IMapper _mapper;

    public AuxiliaryManagersTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<BLLMappingProfile>());
        _mapper = config.CreateMapper();
    }

    // =========================================================================
    // COMPANY MANAGER TESTS
    // =========================================================================

    [Fact]
    public async Task CompanyManager_GetAllCompanies_WhenEF_CallsEFRepository()
    {
        // Arrange
        var efUowMock = new Mock<IEFUnitOfWork>();
        var adoUowMock = new Mock<IUnitOfWork>();
        var efRepoMock = new Mock<IEFCompanyRepository>();
        var loggerMock = new Mock<ILogger<CompanyManager>>();

        efUowMock.Setup(u => u.EFCompanyRepository).Returns(efRepoMock.Object);
        efRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<EFCompany>
        {
            new() { Id = 1, CompanyName = "Транс 1" },
            new() { Id = 2, CompanyName = "Транс 2" }
        });

        var manager = new CompanyManager(loggerMock.Object, _mapper, adoUowMock.Object, efUowMock.Object);

        // Act
        var result = await manager.GetAllCompaniesAsync(DataAccessProvider.EntityFramework);

        // Assert
        result.Should().HaveCount(2);
        efRepoMock.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task CompanyManager_GetAllCompanies_WhenDapper_CallsAdoRepository()
    {
        // Arrange
        var efUowMock = new Mock<IEFUnitOfWork>();
        var adoUowMock = new Mock<IUnitOfWork>();
        var adoRepoMock = new Mock<ICompanyRepository>();
        var loggerMock = new Mock<ILogger<CompanyManager>>();

        adoUowMock.Setup(u => u.CompanyRepository).Returns(adoRepoMock.Object);
        adoRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Company>
        {
            new() { Id = 10, CompanyName = "Експрес Логістик" }
        });

        var manager = new CompanyManager(loggerMock.Object, _mapper, adoUowMock.Object, efUowMock.Object);

        // Act
        var result = await manager.GetAllCompaniesAsync(DataAccessProvider.Dapper);

        // Assert
        result.Should().HaveCount(1);
        result.First().CompanyName.Should().Be("Експрес Логістик");
        adoRepoMock.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task CompanyManager_CreateCompany_WhenEF_SavesAndReturnsDTO()
    {
        // Arrange
        var efUowMock = new Mock<IEFUnitOfWork>();
        var adoUowMock = new Mock<IUnitOfWork>();
        var efRepoMock = new Mock<IEFCompanyRepository>();
        var loggerMock = new Mock<ILogger<CompanyManager>>();

        efUowMock.Setup(u => u.EFCompanyRepository).Returns(efRepoMock.Object);
        efRepoMock.Setup(r => r.AddAsync(It.IsAny<EFCompany>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EFCompany c, CancellationToken _) => { c.Id = 77; return c; });

        var manager = new CompanyManager(loggerMock.Object, _mapper, adoUowMock.Object, efUowMock.Object);
        var req = new CompanyReqDTO { CompanyName = "Нова Компанія" };

        // Act
        var result = await manager.CreateCompanyAsync(req, DataAccessProvider.EntityFramework);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(77);
        result.CompanyName.Should().Be("Нова Компанія");
        efUowMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // =========================================================================
    // EXPENSE MANAGER TESTS
    // =========================================================================

    [Fact]
    public async Task ExpenseManager_GetExpensesByDriver_WhenEF_CallsEFRepository()
    {
        // Arrange
        var efUowMock = new Mock<IEFUnitOfWork>();
        var adoUowMock = new Mock<IUnitOfWork>();
        var efRepoMock = new Mock<IEFExpenseRepository>();
        var loggerMock = new Mock<ILogger<ExpenseManager>>();

        efUowMock.Setup(u => u.EFExpenseRepository).Returns(efRepoMock.Object);
        efRepoMock.Setup(r => r.GetExpensesByDriverIdAsync(5)).ReturnsAsync(new List<EFExpense>
        {
            new() { Id = 1, DriverId = 5, Category = "Пальне", FuelCost = 1200m }
        });

        var manager = new ExpenseManager(loggerMock.Object, _mapper, adoUowMock.Object, efUowMock.Object);

        // Act
        var result = await manager.GetExpensesByDriverIdAsync(5, DataAccessProvider.EntityFramework);

        // Assert
        result.Should().HaveCount(1);
        result.First().FuelCost.Should().Be(1200m);
        efRepoMock.Verify(r => r.GetExpensesByDriverIdAsync(5), Times.Once);
    }

    [Fact]
    public async Task ExpenseManager_GetExpensesByTruck_WhenDapper_CallsAdoRepository()
    {
        // Arrange
        var efUowMock = new Mock<IEFUnitOfWork>();
        var adoUowMock = new Mock<IUnitOfWork>();
        var adoRepoMock = new Mock<IExpensesRepository>();
        var loggerMock = new Mock<ILogger<ExpenseManager>>();

        adoUowMock.Setup(u => u.ExpensesRepository).Returns(adoRepoMock.Object);
        adoRepoMock.Setup(r => r.GetExpensesByTruckIdAsync(12)).ReturnsAsync(new List<Expenses>
        {
            new() { Id = 10, TruckID = 12, Category = "Техобслуговування", MaintenanceCost = 3500m }
        });

        var manager = new ExpenseManager(loggerMock.Object, _mapper, adoUowMock.Object, efUowMock.Object);

        // Act
        var result = await manager.GetExpensesByTruckIdAsync(12, DataAccessProvider.Dapper);

        // Assert
        result.Should().HaveCount(1);
        result.First().MaintenanceCost.Should().Be(3500m);
        adoRepoMock.Verify(r => r.GetExpensesByTruckIdAsync(12), Times.Once);
    }

    // =========================================================================
    // INSPECTION MANAGER TESTS
    // =========================================================================

    [Fact]
    public async Task InspectionManager_GetInspectionsByTruck_WhenEF_CallsEFRepository()
    {
        // Arrange
        var efUowMock = new Mock<IEFUnitOfWork>();
        var adoUowMock = new Mock<IUnitOfWork>();
        var efRepoMock = new Mock<IEFInspectionRepository>();
        var loggerMock = new Mock<ILogger<InspectationManager>>();

        efUowMock.Setup(u => u.EFInspectionRepository).Returns(efRepoMock.Object);
        efRepoMock.Setup(r => r.GetInspectionsByTruckIdAsync(3)).ReturnsAsync(new List<EFInspection>
        {
            new() { Id = 1, TruckId = 3, Result = true, Description = "Пройдено успішно" }
        });

        var manager = new InspectationManager(loggerMock.Object, _mapper, adoUowMock.Object, efUowMock.Object);

        // Act
        var result = await manager.GetInspectionsByTruckIdAsync(3, DataAccessProvider.EntityFramework);

        // Assert
        result.Should().HaveCount(1);
        result.First().Result.Should().BeTrue();
        result.First().Description.Should().Be("Пройдено успішно");
    }

    [Fact]
    public async Task InspectionManager_CreateInspection_WhenDapper_CallsAddAsync()
    {
        // Arrange
        var efUowMock = new Mock<IEFUnitOfWork>();
        var adoUowMock = new Mock<IUnitOfWork>();
        var adoRepoMock = new Mock<IInspectionRepository>();
        var loggerMock = new Mock<ILogger<InspectationManager>>();

        adoUowMock.Setup(u => u.InspectionRepository).Returns(adoRepoMock.Object);
        adoRepoMock.Setup(r => r.AddAsync(It.IsAny<Inspection>())).ReturnsAsync(88);

        var manager = new InspectationManager(loggerMock.Object, _mapper, adoUowMock.Object, efUowMock.Object);
        var req = new InspectionReqDTO { InspectionDate = DateTime.UtcNow, Result = true, TruckId = 4 };

        // Act
        var result = await manager.CreateInspectionAsync(req, DataAccessProvider.Dapper);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(88);
        adoRepoMock.Verify(r => r.AddAsync(It.IsAny<Inspection>()), Times.Once);
    }

    // =========================================================================
    // REPAIR MANAGER TESTS
    // =========================================================================

    [Fact]
    public async Task RepairManager_GetRepairsByTruck_WhenEF_CallsEFRepository()
    {
        // Arrange
        var efUowMock = new Mock<IEFUnitOfWork>();
        var adoUowMock = new Mock<IUnitOfWork>();
        var efRepoMock = new Mock<IEFRepairRepository>();
        var loggerMock = new Mock<ILogger<RepairManager>>();

        efUowMock.Setup(u => u.EFPRepairRepository).Returns(efRepoMock.Object);
        efRepoMock.Setup(r => r.GetRepairsByTruckIdAsync(8)).ReturnsAsync(new List<EFRepair>
        {
            new() { Id = 1, TruckId = 8, Cost = 5000m, Description = "Капітальний ремонт двигуна" }
        });

        var manager = new RepairManager(loggerMock.Object, _mapper, adoUowMock.Object, efUowMock.Object);

        // Act
        var result = await manager.GetRepairsByTruckIdAsync(8, DataAccessProvider.EntityFramework);

        // Assert
        result.Should().HaveCount(1);
        result.First().Cost.Should().Be(5000m);
        result.First().Description.Should().Be("Капітальний ремонт двигуна");
    }

    [Fact]
    public async Task RepairManager_CreateRepair_WhenDapper_CallsAddAsync()
    {
        // Arrange
        var efUowMock = new Mock<IEFUnitOfWork>();
        var adoUowMock = new Mock<IUnitOfWork>();
        var adoRepoMock = new Mock<IRepairRepository>();
        var loggerMock = new Mock<ILogger<RepairManager>>();

        adoUowMock.Setup(u => u.RepairRepository).Returns(adoRepoMock.Object);
        adoRepoMock.Setup(r => r.AddAsync(It.IsAny<Repair>())).ReturnsAsync(99);

        var manager = new RepairManager(loggerMock.Object, _mapper, adoUowMock.Object, efUowMock.Object);
        var req = new RepairReqDTO { RepairDate = DateTime.UtcNow, Cost = 12000m, Description = "Заміна трансмісії", TruckId = 2 };

        // Act
        var result = await manager.CreateRepairAsync(req, DataAccessProvider.Dapper);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(99);
        adoRepoMock.Verify(r => r.AddAsync(It.IsAny<Repair>()), Times.Once);
    }
}

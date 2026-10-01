using AutoMapper;
using Drivers.BLL.DTOs.Responses;
using Drivers.BLL.Mapping;
using FluentAssertions;
using Xunit;
using ADOEntities = Drivers.DAL.ADO.Entities;
using ADOHelp = Drivers.DAL.ADO.Entities.HelpModels;
using EFEntities = Drivers.DAL.EF.Entities;
using EFHelp = Drivers.DAL.EF.Entities.HelpModels;

namespace NtierDrivers.UnitTests;

/// <summary>
/// Тести коректності мапінгу сутностей EF Core та Dapper у DTO
/// </summary>
public class MappingTests
{
    private readonly IMapper _mapper;

    public MappingTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<BLLMappingProfile>();
        });
        _mapper = config.CreateMapper();
    }

    [Fact]
    public void EFDriver_ShouldMapTo_FullDriverResponceDTO_Correctly()
    {
        // Arrange
        var efDriver = new EFEntities.EFDriver
        {
            Id = 1,
            FirstName = "Олександр",
            LastName = "Коваленко",
            Birthdate = new DateTime(1985, 5, 20),
            DriverLicenseNumber = "ВХ123456",
            Company = new EFEntities.EFCompany { Id = 10, CompanyName = "Логістик Плюс" },
            Photo = new EFEntities.EFPhoto { Id = 5, FileName = "photo.jpg" },
            Expenses = new List<EFEntities.EFExpense>
            {
                new() { Id = 101, DriverPayment = 1500m, FuelCost = 800m, MaintenanceCost = 200m, Category = "Рейс" }
            }
        };

        // Act
        var dto = _mapper.Map<FullDriverResponceDTO>(efDriver);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(1);
        dto.FirstName.Should().Be("Олександр");
        dto.LastName.Should().Be("Коваленко");
        dto.Company.Should().NotBeNull();
        dto.Company!.CompanyName.Should().Be("Логістик Плюс");
        dto.Photo.Should().NotBeNull();
        dto.Expenses.Should().HaveCount(1);
        dto.Expenses!.First().TotalExpense.Should().Be(2500m);
    }

    [Fact]
    public void ADODriver_ShouldMapTo_FullDriverResponceDTO_Correctly()
    {
        // Arrange
        var adoDriver = new ADOEntities.Driver
        {
            Id = 2,
            FirstName = "Іван",
            LastName = "Петренко",
            Birthdate = new DateTime(1990, 8, 15),
            DriverLicenseNumber = "АА654321",
            CompanyID = 20,
            PhotoID = 7,
            Company = new ADOEntities.Company { Id = 20, CompanyName = "Транс Карго" },
            Photo = new ADOEntities.Photo { Id = 7, FileName = "ivan.png" },
            Expenses = new List<ADOEntities.Expenses>
            {
                new() { Id = 201, DriverID = 2, DriverPayment = 2000m, FuelCost = 1100m, MaintenanceCost = 300m, Category = "Міжнародний" }
            }
        };

        // Act
        var dto = _mapper.Map<FullDriverResponceDTO>(adoDriver);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(2);
        dto.FirstName.Should().Be("Іван");
        dto.LastName.Should().Be("Петренко");
        dto.CompanyId.Should().Be(20);
        dto.Company.Should().NotBeNull();
        dto.Company!.CompanyName.Should().Be("Транс Карго");
        dto.PhotoId.Should().Be(7);
        dto.Expenses.Should().HaveCount(1);
        dto.Expenses!.First().DriverId.Should().Be(2);
        dto.Expenses!.First().TotalExpense.Should().Be(3400m);
    }

    [Fact]
    public void DriverExpenseSummary_BothProviders_ShouldMapToSameDTO()
    {
        // Arrange
        var efSummary = new EFHelp.DriverExpenseSummary
        {
            DriverId = 1,
            ExpenseCount = 5,
            TotalDriverPayment = 10000m,
            TotalFuelCost = 6000m,
            TotalMaintenanceCost = 2500m,
            GrandTotal = 18500m,
            AverageExpense = 3700m
        };

        var adoSummary = new ADOHelp.DriverExpenseSummary
        {
            DriverId = 1,
            ExpenseCount = 5,
            TotalDriverPayment = 10000m,
            TotalFuelCost = 6000m,
            TotalMaintenanceCost = 2500m,
            GrandTotal = 18500m,
            AverageExpense = 3700m
        };

        // Act
        var efDto = _mapper.Map<DriverExpenseSummaryResponseDTO>(efSummary);
        var adoDto = _mapper.Map<DriverExpenseSummaryResponseDTO>(adoSummary);

        // Assert
        efDto.Should().BeEquivalentTo(adoDto);
        efDto.GrandTotal.Should().Be(18500m);
        efDto.ExpenseCount.Should().Be(5);
    }

    [Fact]
    public void TruckMaintenanceSummary_ShouldMapCorrectly()
    {
        // Arrange
        var summary = new ADOHelp.TruckMaintenanceSummary
        {
            TruckId = 15,
            TruckNumber = "AA1122BB",
            Model = "Volvo FH16",
            TotalMaintenanceCost = 5400m,
            TotalRepairsCost = 8200m,
            TotalInspectionsCount = 4,
            PassedInspectionsCount = 3,
            FailedInspectionsCount = 1
        };

        // Act
        var dto = _mapper.Map<TruckMaintenanceSummaryResponseDTO>(summary);

        // Assert
        dto.TruckId.Should().Be(15);
        dto.TruckNumber.Should().Be("AA1122BB");
        dto.TotalMaintenanceCost.Should().Be(5400m);
        dto.TotalRepairsCost.Should().Be(8200m);
        dto.OverallServiceCost.Should().Be(13600m);
        dto.PassedInspectionsCount.Should().Be(3);
        dto.FailedInspectionsCount.Should().Be(1);
    }

    [Fact]
    public void Company_ShouldMapTo_CompanyResponceDTO_And_CompanyWithDriversResponseDTO()
    {
        var efCompany = new EFEntities.EFCompany
        {
            Id = 1,
            CompanyName = "УкрТрансЛогістик",
            Email = "ukr@trans.ua",
            Driver = new EFEntities.EFDriver { Id = 10, FirstName = "Петро", LastName = "Шевченко" }
        };

        var dto = _mapper.Map<CompanyWithDriversResponseDTO>(efCompany);
        dto.Id.Should().Be(1);
        dto.CompanyName.Should().Be("УкрТрансЛогістик");
        dto.Drivers.Should().HaveCount(1);
        dto.Drivers.First().FirstName.Should().Be("Петро");
    }

    [Fact]
    public void Inspection_ShouldMapTo_InspectionResponceDTO()
    {
        var now = DateTime.UtcNow;
        var inspection = new EFEntities.EFInspection
        {
            Id = 10,
            InspectionDate = now,
            Description = "Плановий ТО",
            Result = true,
            TruckId = 5
        };

        var dto = _mapper.Map<InspectionResponceDTO>(inspection);
        dto.Id.Should().Be(10);
        dto.Description.Should().Be("Плановий ТО");
        dto.Result.Should().BeTrue();
        dto.TruckId.Should().Be(5);
    }

    [Fact]
    public void Repair_ShouldMapTo_RepairResponceDTO()
    {
        var now = DateTime.UtcNow;
        var repair = new ADOEntities.Repair
        {
            Id = 20,
            RepairDate = now,
            Description = "Заміна гальмівних колодок",
            Cost = 3500m,
            TruckID = 3
        };

        var dto = _mapper.Map<RepairResponceDTO>(repair);
        dto.Id.Should().Be(20);
        dto.Description.Should().Be("Заміна гальмівних колодок");
        dto.Cost.Should().Be(3500m);
        dto.TruckId.Should().Be(3);
    }
}

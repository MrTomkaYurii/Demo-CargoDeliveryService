using Drivers.BLL.DTOs.Requests;
using Drivers.BLL.DTOs.Validation;
using Drivers.DAL.EF.Entities.HelpModels;
using FluentAssertions;
using Xunit;

namespace NtierDrivers.UnitTests;

/// <summary>
/// Тести FluentValidation для моделей запитів та фільтрів
/// </summary>
public class ValidationTests
{
    private readonly DriverParametersValidator _driverParamsValidator = new();
    private readonly DateRangeValidator _dateRangeValidator = new();
    private readonly MiniDriverReqDTO_Validator _miniDriverValidator = new();

    [Fact]
    public void DriverParameters_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var parameters = new DriverParameters
        {
            PageNumber = 1,
            PageSize = 20,
            MinYearOfBirth = 1970,
            MaxYearOfBirth = 2000
        };

        // Act
        var result = _driverParamsValidator.Validate(parameters);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void DriverParameters_WithInvalidPageNumber_ShouldFailValidation()
    {
        // Arrange
        var parameters = new DriverParameters
        {
            PageNumber = 0,
            PageSize = 10
        };

        // Act
        var result = _driverParamsValidator.Validate(parameters);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PageNumber");
    }

    [Fact]
    public void DriverParameters_WithMinYearGreaterThanMaxYear_ShouldFailValidation()
    {
        // Arrange
        var parameters = new DriverParameters
        {
            MinYearOfBirth = 2005,
            MaxYearOfBirth = 1990
        };

        // Act
        var result = _driverParamsValidator.Validate(parameters);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void DateRange_WhenFromDateIsAfterToDate_ShouldFailValidation()
    {
        // Arrange
        var dateRange = new DateRange(
            new DateTime(2026, 6, 1),
            new DateTime(2026, 1, 1));

        // Act
        var result = _dateRangeValidator.Validate(dateRange);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void DateRange_WhenDatesAreValid_ShouldPassValidation()
    {
        // Arrange
        var dateRange = new DateRange(
            new DateTime(2026, 1, 1),
            new DateTime(2026, 6, 1));

        // Act
        var result = _dateRangeValidator.Validate(dateRange);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void MiniDriverReqDTO_WithEmptyFirstName_ShouldFailValidation()
    {
        // Arrange
        var dto = new MiniDriverReqDTO
        {
            FirstName = null!,
            LastName = "Іванов",
            MiddleName = "Іванович",
            Gender = "Чоловіча",
            Birthdate = new DateTime(1992, 4, 10),
            DriverLicenseNumber = "АВ123456"
        };

        // Act
        var result = _miniDriverValidator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FirstName");
    }

    [Fact]
    public void CompanyReqDTO_Validation_ShouldValidateCorrectly()
    {
        var validator = new CompanyReqDTO_Validator();

        // Valid
        var valid = new CompanyReqDTO { CompanyName = "ТОВ Логістика", Email = "info@cargo.ua" };
        validator.Validate(valid).IsValid.Should().BeTrue();

        // Invalid: empty name
        var invalidName = new CompanyReqDTO { CompanyName = "" };
        validator.Validate(invalidName).IsValid.Should().BeFalse();

        // Invalid: bad email
        var invalidEmail = new CompanyReqDTO { CompanyName = "Valid Name", Email = "not-an-email" };
        validator.Validate(invalidEmail).IsValid.Should().BeFalse();
    }

    [Fact]
    public void ExpenseReqDTO_Validation_ShouldValidateCorrectly()
    {
        var validator = new ExpenseReqDTO_Validator();

        // Valid
        var valid = new ExpenseReqDTO
        {
            Category = "Пальне",
            DriverPayment = 1000m,
            FuelCost = 500m,
            MaintenanceCost = 200m,
            Date = DateTime.UtcNow,
            DriverId = 1,
            TruckId = 1
        };
        validator.Validate(valid).IsValid.Should().BeTrue();

        // Invalid: negative cost
        var invalidCost = new ExpenseReqDTO
        {
            Category = "Пальне",
            FuelCost = -10m,
            Date = DateTime.UtcNow,
            DriverId = 1,
            TruckId = 1
        };
        validator.Validate(invalidCost).IsValid.Should().BeFalse();

        // Invalid: empty category
        var invalidCategory = new ExpenseReqDTO
        {
            Category = "",
            Date = DateTime.UtcNow,
            DriverId = 1,
            TruckId = 1
        };
        validator.Validate(invalidCategory).IsValid.Should().BeFalse();
    }

    [Fact]
    public void InspectionReqDTO_Validation_ShouldValidateCorrectly()
    {
        var validator = new InspectionReqDTO_Validator();

        // Valid
        var valid = new InspectionReqDTO
        {
            InspectionDate = DateTime.UtcNow,
            TruckId = 5
        };
        validator.Validate(valid).IsValid.Should().BeTrue();

        // Invalid: default date
        var invalidDate = new InspectionReqDTO { InspectionDate = default };
        validator.Validate(invalidDate).IsValid.Should().BeFalse();
    }

    [Fact]
    public void RepairReqDTO_Validation_ShouldValidateCorrectly()
    {
        var validator = new RepairReqDTO_Validator();

        // Valid
        var valid = new RepairReqDTO
        {
            RepairDate = DateTime.UtcNow,
            Cost = 1500m,
            TruckId = 3
        };
        validator.Validate(valid).IsValid.Should().BeTrue();

        // Invalid: negative cost
        var invalidCost = new RepairReqDTO
        {
            RepairDate = DateTime.UtcNow,
            Cost = -50m
        };
        validator.Validate(invalidCost).IsValid.Should().BeFalse();
    }
}

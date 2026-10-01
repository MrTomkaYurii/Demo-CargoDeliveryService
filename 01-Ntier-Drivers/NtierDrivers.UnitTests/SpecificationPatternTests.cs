using Drivers.DAL.EF.Entities;
using Drivers.DAL.EF.Specifications;
using Drivers.DAL.EF.Specifications.Evaluator;
using FluentAssertions;
using Xunit;

namespace NtierDrivers.UnitTests;

/// <summary>
/// Модульні тести для патерну Specification (Специфікація) та SpecificationEvaluator.
/// Перевіряють коректність комбінування Where-критеріїв, сортування та пагінації.
/// </summary>
public class SpecificationPatternTests
{
    private readonly List<EFDriver> _testDrivers;

    public SpecificationPatternTests()
    {
        _testDrivers = new List<EFDriver>
        {
            new() { Id = 1, FirstName = "Олександр", LastName = "Бойко", Birthdate = new DateTime(1985, 5, 10), EmploymentStatus = "Active" },
            new() { Id = 2, FirstName = "Василь", LastName = "Коваленко", Birthdate = new DateTime(1995, 3, 15), EmploymentStatus = "Active" },
            new() { Id = 3, FirstName = "Ігор", LastName = "Мельник", Birthdate = new DateTime(2002, 7, 20), EmploymentStatus = "Inactive" },
            new() { Id = 4, FirstName = "Дмитро", LastName = "Шевченко", Birthdate = new DateTime(1980, 11, 25), EmploymentStatus = "Active" },
            new() { Id = 5, FirstName = "Сергій", LastName = "Бондаренко", Birthdate = new DateTime(1990, 1, 30), EmploymentStatus = "OnLeave" }
        };
    }

    [Fact]
    public void SpecificationEvaluator_WhenCriteriaApplied_FiltersDriversCorrectly()
    {
        // Arrange: Шукаємо тільки активних водіїв з прізвищем, що містить "ко"
        var spec = new DriverFilterSpecification(
            searchLastName: "ко",
            employmentStatus: "Active");

        var query = _testDrivers.AsQueryable();

        // Act
        var result = SpecificationEvaluator<EFDriver>.GetQuery(query, spec).ToList();

        // Assert: Очікуємо "Бойко", "Коваленко", "Шевченко"
        result.Should().HaveCount(3);
        result.Should().OnlyContain(d => d.EmploymentStatus == "Active");
        result.Select(d => d.LastName).Should().Contain(new[] { "Бойко", "Коваленко", "Шевченко" });
    }

    [Fact]
    public void SpecificationEvaluator_WhenBirthYearRangeApplied_FiltersCorrectly()
    {
        // Arrange: Водії народжені між 1980 та 1990 роками
        var spec = new DriverFilterSpecification(
            minBirthYear: 1980,
            maxBirthYear: 1990);

        var query = _testDrivers.AsQueryable();

        // Act
        var result = SpecificationEvaluator<EFDriver>.GetQuery(query, spec).ToList();

        // Assert: Очікуємо Бойко (1985), Шевченко (1980), Бондаренко (1990)
        result.Should().HaveCount(3);
        result.Should().OnlyContain(d => d.Birthdate.Year >= 1980 && d.Birthdate.Year <= 1990);
    }

    [Fact]
    public void SpecificationEvaluator_WhenPagingApplied_ReturnsCorrectSlice()
    {
        // Arrange: Впорядкування за прізвищем, Skip 1, Take 2
        var spec = new DriverFilterSpecification(
            skip: 1,
            take: 2,
            isPaging: true);

        var query = _testDrivers.AsQueryable();

        // Act
        var result = SpecificationEvaluator<EFDriver>.GetQuery(query, spec).ToList();

        // Assert: Впорядковані: Бойко, Бондаренко, Коваленко, Мельник, Шевченко
        // Skip 1 -> Бондаренко, Коваленко
        result.Should().HaveCount(2);
        result[0].LastName.Should().Be("Бондаренко");
        result[1].LastName.Should().Be("Коваленко");
    }

    [Fact]
    public void DriverWithFullDetailsSpecification_WhenInstantiated_ConfiguresAllRequiredIncludes()
    {
        // Arrange & Act
        const int targetDriverId = 42;
        var spec = new DriverWithFullDetailsSpecification(targetDriverId);

        // Assert
        spec.Criteria.Should().NotBeNull();
        spec.Includes.Should().HaveCount(3); // Company, Photo, Expenses
        spec.IncludeStrings.Should().Contain("Expenses.Truck");
    }
}

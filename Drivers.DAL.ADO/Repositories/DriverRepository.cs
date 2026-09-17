using System.Data;
using System.Text;
using Dapper;
using Drivers.DAL.ADO.Entities;
using Drivers.DAL.ADO.Entities.HelpModels;
using Drivers.DAL.ADO.Helpers;
using Drivers.DAL.ADO.Repositories.Contracts;
using Microsoft.Data.SqlClient;

namespace Drivers.DAL.ADO.Repositories;

/// <summary>
/// Репозиторій водіїв на базі ADO.NET та Dapper.
/// Реалізує високопродуктивні оптимізовані параметризовані SQL-запити
/// з бізнес-логікою, ідентичною до EF Core репозиторію.
/// </summary>
public class DriverRepository : GenericRepository<Driver>, IDriverRepository
{
    public DriverRepository(SqlConnection sqlConnection, IDbTransaction dbTransaction)
        : base(sqlConnection, dbTransaction, "Drivers")
    {
    }

    /// <summary>
    /// Отримання водія за Id без додаткових зв'язків
    /// </summary>
    public async Task<Driver?> GetByIdAsync(int id)
    {
        const string sql = @"SELECT * FROM [dbo].[Drivers] WHERE [Id] = @Id";
        return await _sqlConnection.QuerySingleOrDefaultAsync<Driver>(
            sql,
            new { Id = id },
            transaction: _dbTransaction);
    }

    /// <summary>
    /// Отримання повної інформації про водія разом із пов'язаними сутностями:
    /// компанією, фотографією та історією витрат з інформацією про вантажівки.
    /// Використовує Dapper Multiple Result Sets (QueryMultipleAsync) для виконання
    /// одного пакетного запиту до БД замість N+1 запитів.
    /// </summary>
    public async Task<Driver?> GetCompleteEntityAsync(int id)
    {
        const string sql = @"
            -- Запит 1: Водій + Компанія + Фото
            SELECT 
                d.[Id], d.[FirstName], d.[LastName], d.[MiddleName], d.[Gender],
                d.[Birthdate], d.[PlaceOfBirth], d.[Nationality], d.[MaritalStatus],
                d.[IdentificationType], d.[IdentificationNumber], d.[IdentificationExpirationDate],
                d.[Address], d.[Phone], d.[Email], d.[DriverLicenseNumber],
                d.[DriverLicenseCategory], d.[DriverLicenseIssuingDate], d.[DriverLicenseExpirationDate],
                d.[DriverLicenseIssuingAuthority], d.[EmploymentStatus], d.[EmploymentStartDate],
                d.[EmploymentEndDate], d.[CompanyID], d.[PhotoID], d.[CreatedAt], d.[UpdatedAt], d.[DeletedAt],
                c.[Id], c.[CompanyName], c.[Address], c.[Phone], c.[Email],
                c.[ContactPerson], c.[ContactPhone], c.[ContactEmail], c.[CreatedAt], c.[UpdatedAt], c.[DeletedAt],
                p.[Id], p.[PhotoData], p.[ContentType], p.[FileName], p.[FileSize],
                p.[CreatedAt], p.[UpdatedAt], p.[DeletedAt]
            FROM [dbo].[Drivers] d
            LEFT JOIN [dbo].[Companies] c ON d.[CompanyID] = c.[Id]
            LEFT JOIN [dbo].[Photos] p ON d.[PhotoID] = p.[Id]
            WHERE d.[Id] = @Id;

            -- Запит 2: Витрати водія + дані вантажівки
            SELECT 
                e.[Id], e.[DriverPayment], e.[FuelCost], e.[MaintenanceCost],
                e.[Category], e.[Date], e.[Note], e.[DriverID], e.[TruckID],
                e.[CreatedAt], e.[UpdatedAt], e.[DeletedAt],
                t.[Id], t.[TruckNumber], t.[Model], t.[Year], t.[Capacity],
                t.[FuelType], t.[FuelConsumption], t.[RegistrationNumber], t.[VIN],
                t.[EngineNumber], t.[ChassisNumber], t.[InsurancePolicyNumber],
                t.[InsuranceExpirationDate], t.[CreatedAt], t.[UpdatedAt], t.[DeletedAt]
            FROM [dbo].[Expenses] e
            LEFT JOIN [dbo].[Trucks] t ON e.[TruckID] = t.[Id]
            WHERE e.[DriverID] = @Id
            ORDER BY e.[Date] DESC;
        ";

        using var multi = await _sqlConnection.QueryMultipleAsync(
            sql,
            new { Id = id },
            transaction: _dbTransaction);

        var driver = multi.Read<Driver, Company?, Photo?, Driver>((d, c, p) =>
        {
            d.Company = c;
            d.Photo = p;
            return d;
        }, splitOn: "Id,Id").FirstOrDefault();

        if (driver != null)
        {
            var expenses = multi.Read<Expenses, Truck?, Expenses>((e, t) =>
            {
                e.Truck = t;
                return e;
            }, splitOn: "Id").ToList();

            driver.Expenses = expenses;
        }

        return driver;
    }

    /// <summary>
    /// Отримання списку водіїв з пагінацією, фільтрацією за роком народження та прізвищем,
    /// а також безпечним динамічним сортуванням.
    /// </summary>
    public async Task<PagedList<Driver>> GetPaginatedDriversAsync(DriverParameters driverParameters)
    {
        var whereBuilder = new StringBuilder("WHERE 1=1 ");
        var parameters = new DynamicParameters();

        if (driverParameters.MinYearOfBirth > 0)
        {
            whereBuilder.Append("AND YEAR([Birthdate]) >= @MinYear ");
            parameters.Add("MinYear", (int)driverParameters.MinYearOfBirth);
        }

        if (driverParameters.MaxYearOfBirth > 0)
        {
            whereBuilder.Append("AND YEAR([Birthdate]) <= @MaxYear ");
            parameters.Add("MaxYear", (int)driverParameters.MaxYearOfBirth);
        }

        if (!string.IsNullOrWhiteSpace(driverParameters.LastName))
        {
            whereBuilder.Append("AND LOWER([LastName]) LIKE @LastName ");
            parameters.Add("LastName", $"%{driverParameters.LastName.Trim().ToLower()}%");
        }

        // Безпечний парсинг сортування (захист від SQL Injection)
        var safeOrderBy = BuildSafeOrderBy(driverParameters.OrderBy);

        var countSql = $"SELECT COUNT(*) FROM [dbo].[Drivers] {whereBuilder}";
        var totalCount = await _sqlConnection.ExecuteScalarAsync<int>(
            countSql,
            parameters,
            transaction: _dbTransaction);

        var offset = (driverParameters.PageNumber - 1) * driverParameters.PageSize;
        parameters.Add("Offset", offset);
        parameters.Add("PageSize", driverParameters.PageSize);

        var querySql = $@"
            SELECT * FROM [dbo].[Drivers]
            {whereBuilder}
            ORDER BY {safeOrderBy}
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
        ";

        var items = (await _sqlConnection.QueryAsync<Driver>(
            querySql,
            parameters,
            transaction: _dbTransaction)).ToList();

        return new PagedList<Driver>(items, totalCount, driverParameters.PageNumber, driverParameters.PageSize);
    }

    /// <summary>
    /// Агрегований фінансовий аналіз витрат водія (виплати, паливо, ремонт) за період
    /// </summary>
    public async Task<DriverExpenseSummary> GetDriverExpenseSummaryAsync(int driverId, DateTime? fromDate, DateTime? toDate)
    {
        const string sql = @"
            SELECT 
                @DriverId AS DriverId,
                COUNT(Id) AS ExpenseCount,
                ISNULL(SUM(DriverPayment), 0) AS TotalDriverPayment,
                ISNULL(SUM(FuelCost), 0) AS TotalFuelCost,
                ISNULL(SUM(MaintenanceCost), 0) AS TotalMaintenanceCost,
                ISNULL(SUM(DriverPayment + FuelCost + MaintenanceCost), 0) AS GrandTotal,
                CASE 
                    WHEN COUNT(Id) > 0 THEN ISNULL(AVG(DriverPayment + FuelCost + MaintenanceCost), 0)
                    ELSE 0 
                END AS AverageExpense,
                MIN([Date]) AS FirstExpenseDate,
                MAX([Date]) AS LastExpenseDate
            FROM [dbo].[Expenses]
            WHERE [DriverID] = @DriverId
              AND (@FromDate IS NULL OR [Date] >= @FromDate)
              AND (@ToDate IS NULL OR [Date] <= @ToDate);
        ";

        var result = await _sqlConnection.QuerySingleOrDefaultAsync<DriverExpenseSummary>(
            sql,
            new { DriverId = driverId, FromDate = fromDate, ToDate = toDate },
            transaction: _dbTransaction);

        return result ?? new DriverExpenseSummary { DriverId = driverId };
    }

    /// <summary>
    /// Визначення Топ-N водіїв з найбільшими загальними витратами для компанії
    /// </summary>
    public async Task<IEnumerable<TopDriverExpense>> GetTopDriversByExpensesAsync(int topCount)
    {
        const string sql = @"
            SELECT TOP (@TopCount)
                d.[Id] AS DriverId,
                TRIM(CONCAT(d.[FirstName], ' ', d.[LastName])) AS FullName,
                c.[CompanyName] AS CompanyName,
                ISNULL(SUM(e.[DriverPayment] + e.[FuelCost] + e.[MaintenanceCost]), 0) AS TotalExpenses,
                COUNT(e.[Id]) AS ExpensesCount,
                MAX(e.[Date]) AS LastExpenseDate
            FROM [dbo].[Drivers] d
            INNER JOIN [dbo].[Expenses] e ON d.[Id] = e.[DriverID]
            LEFT JOIN [dbo].[Companies] c ON d.[CompanyID] = c.[Id]
            GROUP BY d.[Id], d.[FirstName], d.[LastName], c.[CompanyName]
            ORDER BY TotalExpenses DESC;
        ";

        return await _sqlConnection.QueryAsync<TopDriverExpense>(
            sql,
            new { TopCount = topCount },
            transaction: _dbTransaction);
    }

    /// <summary>
    /// Сповіщення щодо водіїв, термін дії посвідчення яких спливає у найближчі дні
    /// </summary>
    public async Task<IEnumerable<DriverLicenseAlert>> GetDriversWithExpiringLicenseAsync(int daysThreshold)
    {
        const string sql = @"
            SELECT 
                d.[Id] AS DriverId,
                TRIM(CONCAT(d.[FirstName], ' ', d.[LastName])) AS FullName,
                d.[DriverLicenseNumber],
                d.[DriverLicenseCategory],
                d.[DriverLicenseExpirationDate],
                DATEDIFF(day, CAST(GETDATE() AS DATE), d.[DriverLicenseExpirationDate]) AS DaysUntilExpiration,
                d.[Phone],
                d.[Email]
            FROM [dbo].[Drivers] d
            WHERE d.[DriverLicenseExpirationDate] >= CAST(GETDATE() AS DATE)
              AND d.[DriverLicenseExpirationDate] <= DATEADD(day, @DaysThreshold, CAST(GETDATE() AS DATE))
            ORDER BY d.[DriverLicenseExpirationDate] ASC;
        ";

        return await _sqlConnection.QueryAsync<DriverLicenseAlert>(
            sql,
            new { DaysThreshold = daysThreshold },
            transaction: _dbTransaction);
    }

    /// <summary>
    /// Перевірка та валідація параметрів сортування для унеможливлення SQL-ін'єкцій
    /// </summary>
    private static string BuildSafeOrderBy(string? orderBy)
    {
        if (string.IsNullOrWhiteSpace(orderBy))
            return "[LastName] ASC";

        var allowedColumns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "lastname", "[LastName]" },
            { "firstname", "[FirstName]" },
            { "birthdate", "[Birthdate]" },
            { "licenseexpiration", "[DriverLicenseExpirationDate]" },
            { "createdat", "[CreatedAt]" }
        };

        var parts = orderBy.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var columnKey = parts[0].ToLower();
        var direction = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";

        if (allowedColumns.TryGetValue(columnKey, out var safeColumn))
        {
            return $"{safeColumn} {direction}";
        }

        return "[LastName] ASC";
    }
}

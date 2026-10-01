using System.Data;
using Dapper;
using Drivers.DAL.ADO.Entities;
using Drivers.DAL.ADO.Entities.HelpModels;
using Drivers.DAL.ADO.Repositories.Contracts;
using Microsoft.Data.SqlClient;

namespace Drivers.DAL.ADO.Repositories;

/// <summary>
/// Репозиторій вантажівок для ADO.NET / Dapper.
/// Забезпечує комплексну вибірку технічного стану та витрат на автопарк.
/// </summary>
public class TruckRepository : GenericRepository<Truck>, ITruckRepository
{
    public TruckRepository(SqlConnection sqlConnection, IDbTransaction dbTransaction)
        : base(sqlConnection, dbTransaction, "Trucks")
    {
    }

    /// <summary>
    /// Отримання повної інформації про вантажівку, включаючи історію витрат, ремонти та інспекції
    /// </summary>
    public async Task<Truck?> GetCompleteEntityAsync(int id)
    {
        const string sql = @"
            SELECT * FROM [dbo].[Trucks] WHERE [Id] = @Id;
            SELECT * FROM [dbo].[Expenses] WHERE [TruckID] = @Id ORDER BY [Date] DESC;
            SELECT * FROM [dbo].[Repairs] WHERE [TruckID] = @Id ORDER BY [RepairDate] DESC;
            SELECT * FROM [dbo].[Inspections] WHERE [TruckID] = @Id ORDER BY [InspectionDate] DESC;
        ";

        using var multi = await _sqlConnection.QueryMultipleAsync(sql, new { Id = id }, transaction: _dbTransaction);
        var truck = await multi.ReadSingleOrDefaultAsync<Truck>();
        if (truck != null)
        {
            truck.Expenses = (await multi.ReadAsync<Expenses>()).ToList();
            truck.Repairs = (await multi.ReadAsync<Repair>()).ToList();
            truck.Inspections = (await multi.ReadAsync<Inspection>()).ToList();
        }

        return truck;
    }

    /// <summary>
    /// Отримання агрегованої аналітики витрат на обслуговування, ремонти та результатів техоглядів
    /// </summary>
    public async Task<TruckMaintenanceSummary?> GetTruckMaintenanceSummaryAsync(int truckId)
    {
        const string sql = @"
            SELECT 
                t.[Id] AS TruckId,
                t.[TruckNumber],
                t.[Model],
                ISNULL((SELECT SUM(e.[MaintenanceCost]) FROM [dbo].[Expenses] e WHERE e.[TruckID] = t.[Id]), 0) AS TotalMaintenanceCost,
                ISNULL((SELECT SUM(r.[Cost]) FROM [dbo].[Repairs] r WHERE r.[TruckID] = t.[Id]), 0) AS TotalRepairsCost,
                ISNULL((SELECT COUNT(i.[Id]) FROM [dbo].[Inspections] i WHERE i.[TruckID] = t.[Id]), 0) AS TotalInspectionsCount,
                ISNULL((SELECT COUNT(i.[Id]) FROM [dbo].[Inspections] i WHERE i.[TruckID] = t.[Id] AND i.[Result] = 1), 0) AS PassedInspectionsCount,
                ISNULL((SELECT COUNT(i.[Id]) FROM [dbo].[Inspections] i WHERE i.[TruckID] = t.[Id] AND i.[Result] = 0), 0) AS FailedInspectionsCount
            FROM [dbo].[Trucks] t
            WHERE t.[Id] = @TruckId;
        ";

        return await _sqlConnection.QuerySingleOrDefaultAsync<TruckMaintenanceSummary>(
            sql,
            new { TruckId = truckId },
            transaction: _dbTransaction);
    }
}

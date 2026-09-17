using Drivers.DAL.ADO.Entities;
using Drivers.DAL.ADO.Repositories.Contracts;
using Microsoft.Data.SqlClient;
using System.Data;

using Dapper;

namespace Drivers.DAL.ADO.Repositories;

public class ExpensesRepository : GenericRepository<Expenses>, IExpensesRepository
{
    public ExpensesRepository(SqlConnection sqlConnection, IDbTransaction dbtransaction) : base(sqlConnection, dbtransaction, "Expenses")
    {
    }

    /// <summary>
    /// Отримання списку витрат за ідентифікатором водія
    /// </summary>
    public async Task<IEnumerable<Expenses>> GetExpensesByDriverIdAsync(int driverId)
    {
        var sql = "SELECT * FROM [Expenses] WHERE [DriverID] = @DriverId";
        return await _sqlConnection.QueryAsync<Expenses>(sql, new { DriverId = driverId }, transaction: _dbTransaction);
    }

    /// <summary>
    /// Отримання списку витрат за ідентифікатором вантажівки
    /// </summary>
    public async Task<IEnumerable<Expenses>> GetExpensesByTruckIdAsync(int truckId)
    {
        var sql = "SELECT * FROM [Expenses] WHERE [TruckID] = @TruckId";
        return await _sqlConnection.QueryAsync<Expenses>(sql, new { TruckId = truckId }, transaction: _dbTransaction);
    }
}

using Drivers.DAL.ADO.Entities;
using Drivers.DAL.ADO.Repositories.Contracts;
using Microsoft.Data.SqlClient;
using System.Data;

using Dapper;

namespace Drivers.DAL.ADO.Repositories;

public class RepairRepository : GenericRepository<Repair>, IRepairRepository
{
    public RepairRepository(SqlConnection sqlConnection, IDbTransaction dbtransaction) : base(sqlConnection, dbtransaction, "Repairs")
    {
    }

    /// <summary>
    /// Отримання списку ремонтів для конкретної вантажівки
    /// </summary>
    public async Task<IEnumerable<Repair>> GetRepairsByTruckIdAsync(int truckId)
    {
        var sql = "SELECT * FROM [Repairs] WHERE [TruckID] = @TruckId";
        return await _sqlConnection.QueryAsync<Repair>(sql, new { TruckId = truckId }, transaction: _dbTransaction);
    }
}

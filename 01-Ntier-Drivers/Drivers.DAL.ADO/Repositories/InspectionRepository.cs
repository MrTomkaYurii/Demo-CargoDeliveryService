using Drivers.DAL.ADO.Entities;
using Drivers.DAL.ADO.Repositories.Contracts;
using Microsoft.Data.SqlClient;
using System.Data;

using Dapper;

namespace Drivers.DAL.ADO.Repositories;

public class InspectionRepository : GenericRepository<Inspection>, IInspectionRepository
{
    public InspectionRepository(SqlConnection sqlConnection, IDbTransaction dbtransaction) : base(sqlConnection, dbtransaction, "Inspections")
    {
    }

    /// <summary>
    /// Отримання списку техоглядів для конкретної вантажівки
    /// </summary>
    public async Task<IEnumerable<Inspection>> GetInspectionsByTruckIdAsync(int truckId)
    {
        var sql = "SELECT * FROM [Inspections] WHERE [TruckID] = @TruckId";
        return await _sqlConnection.QueryAsync<Inspection>(sql, new { TruckId = truckId }, transaction: _dbTransaction);
    }
}

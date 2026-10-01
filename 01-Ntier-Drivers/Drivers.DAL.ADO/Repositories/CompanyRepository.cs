using Drivers.DAL.ADO.Entities;
using Drivers.DAL.ADO.Repositories.Contracts;
using Microsoft.Data.SqlClient;
using System.Data;
using Dapper;
using System.Linq;

namespace Drivers.DAL.ADO.Repositories;

public class CompanyRepository : GenericRepository<Company>, ICompanyRepository
{
    public CompanyRepository(SqlConnection sqlConnection, IDbTransaction dbtransaction) : base(sqlConnection, dbtransaction, "Companies")
    {
    }

    /// <summary>
    /// Отримання компанії разом із закріпленими водіями (Dapper Multi-Mapping)
    /// </summary>
    public async Task<Company?> GetCompanyWithDriversAsync(int companyId)
    {
        var sql = @"
            SELECT c.*, d.*
            FROM [Companies] c
            LEFT JOIN [Drivers] d ON d.[CompanyID] = c.[Id]
            WHERE c.[Id] = @CompanyId";

        Company? company = null;
        await _sqlConnection.QueryAsync<Company, Driver, Company>(
            sql,
            (c, d) =>
            {
                if (company == null)
                {
                    company = c;
                    company.Drivers = new List<Driver>();
                }
                if (d != null && !company.Drivers.Any(x => x.Id == d.Id))
                {
                    company.Drivers.Add(d);
                }
                return company;
            },
            new { CompanyId = companyId },
            transaction: _dbTransaction,
            splitOn: "Id");

        return company;
    }
}

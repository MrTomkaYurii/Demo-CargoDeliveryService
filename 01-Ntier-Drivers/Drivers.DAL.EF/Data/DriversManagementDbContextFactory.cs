using Microsoft.EntityFrameworkCore;
using Reservoom.DbContexts;

namespace Drivers.DAL.EF.Data;

public class DriversManagementDbContextFactory : IDriversManagementDbContextFactory
{
    private readonly string _connectionString;

    public DriversManagementDbContextFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public DriversManagementContext CreateDbContext()
    {
        DbContextOptionsBuilder<DriversManagementContext> options = new();
        options.UseSqlServer(_connectionString);
        return new DriversManagementContext(options.Options);
    }
}

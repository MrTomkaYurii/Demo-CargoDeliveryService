using Drivers.DAL.EF.Data;

namespace Reservoom.DbContexts
{
    public interface IDriversManagementDbContextFactory
    {
        DriversManagementContext CreateDbContext();
    }
}
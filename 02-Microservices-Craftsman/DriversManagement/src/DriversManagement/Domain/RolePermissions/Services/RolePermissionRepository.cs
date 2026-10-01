namespace DriversManagement.Domain.RolePermissions.Services;

using DriversManagement.Domain.RolePermissions;
using DriversManagement.Databases;
using DriversManagement.Services;

public interface IRolePermissionRepository : IGenericRepository<RolePermission>
{
}

public sealed class RolePermissionRepository : GenericRepository<RolePermission>, IRolePermissionRepository
{
    private readonly DriversManagementContext _dbContext;

    public RolePermissionRepository(DriversManagementContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
}

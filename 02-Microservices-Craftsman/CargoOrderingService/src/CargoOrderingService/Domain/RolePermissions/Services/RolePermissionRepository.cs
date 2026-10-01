namespace CargoOrderingService.Domain.RolePermissions.Services;

using CargoOrderingService.Domain.RolePermissions;
using CargoOrderingService.Databases;
using CargoOrderingService.Services;

public interface IRolePermissionRepository : IGenericRepository<RolePermission>
{
}

public sealed class RolePermissionRepository : GenericRepository<RolePermission>, IRolePermissionRepository
{
    private readonly OrderingContext _dbContext;

    public RolePermissionRepository(OrderingContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
}

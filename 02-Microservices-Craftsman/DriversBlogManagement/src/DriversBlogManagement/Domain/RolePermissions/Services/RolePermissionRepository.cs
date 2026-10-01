namespace DriversBlogManagement.Domain.RolePermissions.Services;

using DriversBlogManagement.Domain.RolePermissions;
using DriversBlogManagement.Databases;
using DriversBlogManagement.Services;

public interface IRolePermissionRepository : IGenericRepository<RolePermission>
{
}

public sealed class RolePermissionRepository : GenericRepository<RolePermission>, IRolePermissionRepository
{
    private readonly BlogManagementContext _dbContext;

    public RolePermissionRepository(BlogManagementContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
}

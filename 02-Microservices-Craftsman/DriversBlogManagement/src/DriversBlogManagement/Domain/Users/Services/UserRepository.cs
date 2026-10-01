namespace DriversBlogManagement.Domain.Users.Services;

using Microsoft.EntityFrameworkCore;
using DriversBlogManagement.Domain.Users;
using DriversBlogManagement.Databases;
using DriversBlogManagement.Services;

public interface IUserRepository : IGenericRepository<User>
{
    public IEnumerable<string> GetRolesByUserIdentifier(string identifier);
    public Task AddRole(UserRole entity, CancellationToken cancellationToken = default);
    public void RemoveRole(UserRole entity);
}

public class UserRepository : GenericRepository<User>, IUserRepository
{
    private readonly BlogManagementContext _dbContext;

    public UserRepository(BlogManagementContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task<User> GetByIdOrDefault(Guid id, bool withTracking = true, CancellationToken cancellationToken = default)
    {
        return withTracking 
            ? await _dbContext.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken) 
            : await _dbContext.Users
                .Include(u => u.Roles)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task AddRole(UserRole userRole, CancellationToken cancellationToken = default)
    {
        await _dbContext.UserRoles.AddAsync(userRole, cancellationToken);
    }

    public void RemoveRole(UserRole userRole)
    {
        _dbContext.UserRoles.Remove(userRole);
    }

    public IEnumerable<string> GetRolesByUserIdentifier(string identifier)
    {
        return _dbContext.UserRoles
            .Include(x => x.User)
            .Where(x => x.User.Identifier == identifier)
            .Select(x => x.Role.Value);
    }
}

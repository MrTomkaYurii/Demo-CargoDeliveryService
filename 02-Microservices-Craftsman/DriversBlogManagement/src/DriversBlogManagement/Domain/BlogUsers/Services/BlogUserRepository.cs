namespace DriversBlogManagement.Domain.BlogUsers.Services;

using DriversBlogManagement.Domain.BlogUsers;
using DriversBlogManagement.Databases;
using DriversBlogManagement.Services;

public interface IBlogUserRepository : IGenericRepository<BlogUser>
{
}

public sealed class BlogUserRepository : GenericRepository<BlogUser>, IBlogUserRepository
{
    private readonly BlogManagementContext _dbContext;

    public BlogUserRepository(BlogManagementContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
}

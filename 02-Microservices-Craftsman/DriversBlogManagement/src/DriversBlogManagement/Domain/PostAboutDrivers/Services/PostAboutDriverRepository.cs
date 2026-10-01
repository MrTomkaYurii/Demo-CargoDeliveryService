namespace DriversBlogManagement.Domain.PostAboutDrivers.Services;

using DriversBlogManagement.Domain.PostAboutDrivers;
using DriversBlogManagement.Databases;
using DriversBlogManagement.Services;

public interface IPostAboutDriverRepository : IGenericRepository<PostAboutDriver>
{
}

public sealed class PostAboutDriverRepository : GenericRepository<PostAboutDriver>, IPostAboutDriverRepository
{
    private readonly BlogManagementContext _dbContext;

    public PostAboutDriverRepository(BlogManagementContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
}

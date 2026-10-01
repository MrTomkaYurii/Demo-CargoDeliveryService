namespace DriversBlogManagement.Domain.Likes.Services;

using DriversBlogManagement.Domain.Likes;
using DriversBlogManagement.Databases;
using DriversBlogManagement.Services;

public interface ILikeRepository : IGenericRepository<Like>
{
}

public sealed class LikeRepository : GenericRepository<Like>, ILikeRepository
{
    private readonly BlogManagementContext _dbContext;

    public LikeRepository(BlogManagementContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
}

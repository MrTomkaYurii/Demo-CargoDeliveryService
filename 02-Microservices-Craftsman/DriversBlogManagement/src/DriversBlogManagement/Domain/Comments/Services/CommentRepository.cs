namespace DriversBlogManagement.Domain.Comments.Services;

using DriversBlogManagement.Domain.Comments;
using DriversBlogManagement.Databases;
using DriversBlogManagement.Services;

public interface ICommentRepository : IGenericRepository<Comment>
{
}

public sealed class CommentRepository : GenericRepository<Comment>, ICommentRepository
{
    private readonly BlogManagementContext _dbContext;

    public CommentRepository(BlogManagementContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
}

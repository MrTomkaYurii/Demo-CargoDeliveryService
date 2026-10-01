namespace DriversManagement.Domain.Photos.Services;

using DriversManagement.Domain.Photos;
using DriversManagement.Databases;
using DriversManagement.Services;

public interface IPhotoRepository : IGenericRepository<Photo>
{
}

public sealed class PhotoRepository : GenericRepository<Photo>, IPhotoRepository
{
    private readonly DriversManagementContext _dbContext;

    public PhotoRepository(DriversManagementContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
}

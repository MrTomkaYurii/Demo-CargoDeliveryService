using Drivers.DAL.EF.Data;
using Drivers.DAL.EF.Entities;
using Drivers.DAL.EF.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Drivers.DAL.EF.Repositories;

/// <summary>
/// Репозиторій фотографій для Entity Framework Core
/// </summary>
public class EFPhotoRepository : EFGenericRepository<EFPhoto>, IEFPhotoRepository
{
    public EFPhotoRepository(DriversManagementContext databaseContext) : base(databaseContext)
    {
    }

    /// <summary>
    /// Отримання фотографії разом із закріпленим водієм
    /// </summary>
    public override async Task<EFPhoto?> GetCompleteEntityAsync(int id, CancellationToken cancellationToken = default)
    {
        return await table
            .AsNoTracking()
            .Include(p => p.Driver)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }
}

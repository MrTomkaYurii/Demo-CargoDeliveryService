using Drivers.DAL.EF.Data;
using Drivers.DAL.EF.Entities;
using Drivers.DAL.EF.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Drivers.DAL.EF.Repositories;

/// <summary>
/// Репозиторій компаній для Entity Framework Core
/// </summary>
public class EFCompanyRepository : EFGenericRepository<EFCompany>, IEFCompanyRepository
{
    public EFCompanyRepository(DriversManagementContext databaseContext) : base(databaseContext)
    {
    }

    /// <summary>
    /// Отримання повної інформації про компанію разом із закріпленим водієм
    /// </summary>
    public override async Task<EFCompany?> GetCompleteEntityAsync(int id)
    {
        return await table
            .AsNoTracking()
            .Include(c => c.Driver)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    /// <summary>
    /// Отримання компанії з інформацією про водіїв
    /// </summary>
    public async Task<EFCompany?> GetCompanyWithDriversAsync(int companyId)
    {
        return await table
            .AsNoTracking()
            .Include(c => c.Driver)
            .FirstOrDefaultAsync(c => c.Id == companyId);
    }
}

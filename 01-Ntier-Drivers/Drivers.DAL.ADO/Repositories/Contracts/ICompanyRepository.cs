using Drivers.DAL.ADO.Entities;

namespace Drivers.DAL.ADO.Repositories.Contracts;

public interface ICompanyRepository : IGenericRepository<Company>
{
    Task<Company?> GetCompanyWithDriversAsync(int companyId);
}

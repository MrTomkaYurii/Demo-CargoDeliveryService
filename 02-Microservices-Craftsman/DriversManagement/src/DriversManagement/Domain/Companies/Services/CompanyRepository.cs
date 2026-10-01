namespace DriversManagement.Domain.Companies.Services;

using DriversManagement.Domain.Companies;
using DriversManagement.Databases;
using DriversManagement.Services;

public interface ICompanyRepository : IGenericRepository<Company>
{
}

public sealed class CompanyRepository : GenericRepository<Company>, ICompanyRepository
{
    private readonly DriversManagementContext _dbContext;

    public CompanyRepository(DriversManagementContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
}

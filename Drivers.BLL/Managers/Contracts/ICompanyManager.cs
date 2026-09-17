using Drivers.BLL.DTOs.Requests;
using Drivers.BLL.DTOs.Responses;
using Drivers.BLL.Models;

namespace Drivers.BLL.Managers.Contracts;

public interface ICompanyManager
{
    Task<CompanyResponceDTO?> GetCompanyById(int id);
    Task<IEnumerable<CompanyResponceDTO>> GetAllCompaniesAsync(DataAccessProvider provider, CancellationToken cancellationToken = default);
    Task<CompanyResponceDTO?> GetCompanyByIdAsync(int id, DataAccessProvider provider, CancellationToken cancellationToken = default);
    Task<CompanyWithDriversResponseDTO?> GetCompanyWithDriversAsync(int id, DataAccessProvider provider, CancellationToken cancellationToken = default);
    Task<CompanyResponceDTO> CreateCompanyAsync(CompanyReqDTO dto, DataAccessProvider provider, CancellationToken cancellationToken = default);
    Task<CompanyResponceDTO?> UpdateCompanyAsync(int id, CompanyReqDTO dto, DataAccessProvider provider, CancellationToken cancellationToken = default);
    Task<bool> DeleteCompanyAsync(int id, DataAccessProvider provider, CancellationToken cancellationToken = default);
}

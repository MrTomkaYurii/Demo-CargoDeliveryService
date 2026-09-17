using Drivers.BLL.DTOs.Requests;
using Drivers.BLL.DTOs.Responses;
using Drivers.BLL.Models;

namespace Drivers.BLL.Managers.Contracts;

public interface IInspectationManager
{
    Task<IEnumerable<InspectionResponceDTO>> GetAllInspectionsAsync(DataAccessProvider provider, CancellationToken cancellationToken = default);
    Task<InspectionResponceDTO?> GetInspectionByIdAsync(int id, DataAccessProvider provider, CancellationToken cancellationToken = default);
    Task<IEnumerable<InspectionResponceDTO>> GetInspectionsByTruckIdAsync(int truckId, DataAccessProvider provider, CancellationToken cancellationToken = default);
    Task<InspectionResponceDTO> CreateInspectionAsync(InspectionReqDTO dto, DataAccessProvider provider, CancellationToken cancellationToken = default);
    Task<InspectionResponceDTO?> UpdateInspectionAsync(int id, InspectionReqDTO dto, DataAccessProvider provider, CancellationToken cancellationToken = default);
    Task<bool> DeleteInspectionAsync(int id, DataAccessProvider provider, CancellationToken cancellationToken = default);
}

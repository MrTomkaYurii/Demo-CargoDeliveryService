using Drivers.BLL.DTOs.Requests;
using Drivers.BLL.DTOs.Responses;
using Drivers.BLL.Models;

namespace Drivers.BLL.Managers.Contracts;

public interface IRepairManager
{
    Task<IEnumerable<RepairResponceDTO>> GetAllRepairsAsync(DataAccessProvider provider, CancellationToken cancellationToken = default);
    Task<RepairResponceDTO?> GetRepairByIdAsync(int id, DataAccessProvider provider, CancellationToken cancellationToken = default);
    Task<IEnumerable<RepairResponceDTO>> GetRepairsByTruckIdAsync(int truckId, DataAccessProvider provider, CancellationToken cancellationToken = default);
    Task<RepairResponceDTO> CreateRepairAsync(RepairReqDTO dto, DataAccessProvider provider, CancellationToken cancellationToken = default);
    Task<RepairResponceDTO?> UpdateRepairAsync(int id, RepairReqDTO dto, DataAccessProvider provider, CancellationToken cancellationToken = default);
    Task<bool> DeleteRepairAsync(int id, DataAccessProvider provider, CancellationToken cancellationToken = default);
}

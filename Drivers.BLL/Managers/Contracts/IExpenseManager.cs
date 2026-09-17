using Drivers.BLL.DTOs.Requests;
using Drivers.BLL.DTOs.Responses;
using Drivers.BLL.Models;

namespace Drivers.BLL.Managers.Contracts;

public interface IExpenseManager
{
    Task<IEnumerable<ExpenseResponceDTO>> GetAllExpensesAsync(DataAccessProvider provider, CancellationToken cancellationToken = default);
    Task<ExpenseResponceDTO?> GetExpenseByIdAsync(int id, DataAccessProvider provider, CancellationToken cancellationToken = default);
    Task<IEnumerable<ExpenseResponceDTO>> GetExpensesByDriverIdAsync(int driverId, DataAccessProvider provider, CancellationToken cancellationToken = default);
    Task<IEnumerable<ExpenseResponceDTO>> GetExpensesByTruckIdAsync(int truckId, DataAccessProvider provider, CancellationToken cancellationToken = default);
    Task<ExpenseResponceDTO> CreateExpenseAsync(ExpenseReqDTO dto, DataAccessProvider provider, CancellationToken cancellationToken = default);
    Task<ExpenseResponceDTO?> UpdateExpenseAsync(int id, ExpenseReqDTO dto, DataAccessProvider provider, CancellationToken cancellationToken = default);
    Task<bool> DeleteExpenseAsync(int id, DataAccessProvider provider, CancellationToken cancellationToken = default);
}

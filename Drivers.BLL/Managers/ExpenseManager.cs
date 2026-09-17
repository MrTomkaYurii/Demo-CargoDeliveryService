using AutoMapper;
using Drivers.BLL.DTOs.Requests;
using Drivers.BLL.DTOs.Responses;
using Drivers.BLL.Managers.Contracts;
using Drivers.BLL.Models;
using Drivers.DAL.ADO.Entities;
using Drivers.DAL.ADO.UOW.Contracts;
using Drivers.DAL.EF.Entities;
using Drivers.DAL.EF.UOW.Contracts;
using Microsoft.Extensions.Logging;

namespace Drivers.BLL.Managers;

/// <summary>
/// Менеджер витрат. Підтримує виклики через EF Core та Dapper.
/// </summary>
public class ExpenseManager : IExpenseManager
{
    private readonly ILogger<ExpenseManager> _logger;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _adoUow;
    private readonly IEFUnitOfWork _efUow;

    public ExpenseManager(
        ILogger<ExpenseManager> logger,
        IMapper mapper,
        IUnitOfWork adoUow,
        IEFUnitOfWork efUow)
    {
        _logger = logger;
        _mapper = mapper;
        _adoUow = adoUow;
        _efUow = efUow;
    }

    /// <summary>
    /// Отримання списку всіх витрат
    /// </summary>
    public async Task<IEnumerable<ExpenseResponceDTO>> GetAllExpensesAsync(
        DataAccessProvider provider,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Отримання всіх витрат через {Provider}", provider);

        if (provider == DataAccessProvider.Dapper)
        {
            var expenses = await _adoUow.ExpensesRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ExpenseResponceDTO>>(expenses);
        }
        else
        {
            var expenses = await _efUow.EFExpenseRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ExpenseResponceDTO>>(expenses);
        }
    }

    /// <summary>
    /// Отримання витрат за ідентифікатором
    /// </summary>
    public async Task<ExpenseResponceDTO?> GetExpenseByIdAsync(
        int id,
        DataAccessProvider provider,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Отримання витрат {Id} через {Provider}", id, provider);

        if (provider == DataAccessProvider.Dapper)
        {
            try
            {
                var expense = await _adoUow.ExpensesRepository.GetAsync(id);
                return _mapper.Map<ExpenseResponceDTO>(expense);
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }
        else
        {
            try
            {
                var expense = await _efUow.EFExpenseRepository.GetCompleteEntityAsync(id);
                return expense != null ? _mapper.Map<ExpenseResponceDTO>(expense) : null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Отримання всіх витрат для конкретного водія
    /// </summary>
    public async Task<IEnumerable<ExpenseResponceDTO>> GetExpensesByDriverIdAsync(
        int driverId,
        DataAccessProvider provider,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Отримання витрат водія {DriverId} через {Provider}", driverId, provider);

        if (provider == DataAccessProvider.Dapper)
        {
            var expenses = await _adoUow.ExpensesRepository.GetExpensesByDriverIdAsync(driverId);
            return _mapper.Map<IEnumerable<ExpenseResponceDTO>>(expenses);
        }
        else
        {
            var expenses = await _efUow.EFExpenseRepository.GetExpensesByDriverIdAsync(driverId);
            return _mapper.Map<IEnumerable<ExpenseResponceDTO>>(expenses);
        }
    }

    /// <summary>
    /// Отримання всіх витрат для конкретної вантажівки
    /// </summary>
    public async Task<IEnumerable<ExpenseResponceDTO>> GetExpensesByTruckIdAsync(
        int truckId,
        DataAccessProvider provider,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Отримання витрат вантажівки {TruckId} через {Provider}", truckId, provider);

        if (provider == DataAccessProvider.Dapper)
        {
            var expenses = await _adoUow.ExpensesRepository.GetExpensesByTruckIdAsync(truckId);
            return _mapper.Map<IEnumerable<ExpenseResponceDTO>>(expenses);
        }
        else
        {
            var expenses = await _efUow.EFExpenseRepository.GetExpensesByTruckIdAsync(truckId);
            return _mapper.Map<IEnumerable<ExpenseResponceDTO>>(expenses);
        }
    }

    /// <summary>
    /// Створення нового запису витрат
    /// </summary>
    public async Task<ExpenseResponceDTO> CreateExpenseAsync(
        ExpenseReqDTO dto,
        DataAccessProvider provider,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Створення витрат через {Provider}", provider);

        if (provider == DataAccessProvider.Dapper)
        {
            var entity = _mapper.Map<Expenses>(dto);
            entity.CreatedAt = DateTime.UtcNow;
            var newId = await _adoUow.ExpensesRepository.AddAsync(entity);
            entity.Id = newId;
            return _mapper.Map<ExpenseResponceDTO>(entity);
        }
        else
        {
            var entity = _mapper.Map<EFExpense>(dto);
            entity.CreatedAt = DateTime.UtcNow;
            var added = await _efUow.EFExpenseRepository.AddAsync(entity);
            await _efUow.CompleteAsync(cancellationToken);
            return _mapper.Map<ExpenseResponceDTO>(added);
        }
    }

    /// <summary>
    /// Оновлення запису витрат
    /// </summary>
    public async Task<ExpenseResponceDTO?> UpdateExpenseAsync(
        int id,
        ExpenseReqDTO dto,
        DataAccessProvider provider,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Оновлення витрат {Id} через {Provider}", id, provider);

        if (provider == DataAccessProvider.Dapper)
        {
            try
            {
                var existing = await _adoUow.ExpensesRepository.GetAsync(id);
                _mapper.Map(dto, existing);
                existing.Id = id;
                existing.UpdatedAt = DateTime.UtcNow;
                await _adoUow.ExpensesRepository.ReplaceAsync(existing);
                return _mapper.Map<ExpenseResponceDTO>(existing);
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }
        else
        {
            try
            {
                var existing = await _efUow.EFExpenseRepository.GetByIdAsync(id);
                _mapper.Map(dto, existing);
                existing.Id = id;
                existing.UpdatedAt = DateTime.UtcNow;
                await _efUow.EFExpenseRepository.UpdateAsync(existing);
                await _efUow.CompleteAsync(cancellationToken);
                return _mapper.Map<ExpenseResponceDTO>(existing);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Видалення запису витрат
    /// </summary>
    public async Task<bool> DeleteExpenseAsync(
        int id,
        DataAccessProvider provider,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Видалення витрат {Id} через {Provider}", id, provider);

        if (provider == DataAccessProvider.Dapper)
        {
            try
            {
                await _adoUow.ExpensesRepository.GetAsync(id);
                await _adoUow.ExpensesRepository.DeleteAsync(id);
                return true;
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
        }
        else
        {
            try
            {
                await _efUow.EFExpenseRepository.DeleteByIdAsync(id);
                await _efUow.CompleteAsync(cancellationToken);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

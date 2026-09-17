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
/// Менеджер ремонтів. Підтримує виклики через EF Core та Dapper.
/// </summary>
public class RepairManager : IRepairManager
{
    private readonly ILogger<RepairManager> _logger;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _adoUow;
    private readonly IEFUnitOfWork _efUow;

    public RepairManager(
        ILogger<RepairManager> logger,
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
    /// Отримання всіх ремонтів
    /// </summary>
    public async Task<IEnumerable<RepairResponceDTO>> GetAllRepairsAsync(
        DataAccessProvider provider,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Отримання всіх ремонтів через {Provider}", provider);

        if (provider == DataAccessProvider.Dapper)
        {
            var repairs = await _adoUow.RepairRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<RepairResponceDTO>>(repairs);
        }
        else
        {
            var repairs = await _efUow.EFPRepairRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<RepairResponceDTO>>(repairs);
        }
    }

    /// <summary>
    /// Отримання ремонту за ідентифікатором
    /// </summary>
    public async Task<RepairResponceDTO?> GetRepairByIdAsync(
        int id,
        DataAccessProvider provider,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Отримання ремонту {Id} через {Provider}", id, provider);

        if (provider == DataAccessProvider.Dapper)
        {
            try
            {
                var repair = await _adoUow.RepairRepository.GetAsync(id);
                return _mapper.Map<RepairResponceDTO>(repair);
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
                var repair = await _efUow.EFPRepairRepository.GetCompleteEntityAsync(id);
                return repair != null ? _mapper.Map<RepairResponceDTO>(repair) : null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Отримання списку ремонтів для конкретної вантажівки
    /// </summary>
    public async Task<IEnumerable<RepairResponceDTO>> GetRepairsByTruckIdAsync(
        int truckId,
        DataAccessProvider provider,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Отримання ремонтів для вантажівки {TruckId} через {Provider}", truckId, provider);

        if (provider == DataAccessProvider.Dapper)
        {
            var repairs = await _adoUow.RepairRepository.GetRepairsByTruckIdAsync(truckId);
            return _mapper.Map<IEnumerable<RepairResponceDTO>>(repairs);
        }
        else
        {
            var repairs = await _efUow.EFPRepairRepository.GetRepairsByTruckIdAsync(truckId);
            return _mapper.Map<IEnumerable<RepairResponceDTO>>(repairs);
        }
    }

    /// <summary>
    /// Створення нового ремонту
    /// </summary>
    public async Task<RepairResponceDTO> CreateRepairAsync(
        RepairReqDTO dto,
        DataAccessProvider provider,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Створення ремонту через {Provider}", provider);

        if (provider == DataAccessProvider.Dapper)
        {
            var entity = _mapper.Map<Repair>(dto);
            entity.CreatedAt = DateTime.UtcNow;
            var newId = await _adoUow.RepairRepository.AddAsync(entity);
            entity.Id = newId;
            return _mapper.Map<RepairResponceDTO>(entity);
        }
        else
        {
            var entity = _mapper.Map<EFRepair>(dto);
            entity.CreatedAt = DateTime.UtcNow;
            var added = await _efUow.EFPRepairRepository.AddAsync(entity);
            await _efUow.CompleteAsync(cancellationToken);
            return _mapper.Map<RepairResponceDTO>(added);
        }
    }

    /// <summary>
    /// Оновлення ремонту
    /// </summary>
    public async Task<RepairResponceDTO?> UpdateRepairAsync(
        int id,
        RepairReqDTO dto,
        DataAccessProvider provider,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Оновлення ремонту {Id} через {Provider}", id, provider);

        if (provider == DataAccessProvider.Dapper)
        {
            try
            {
                var existing = await _adoUow.RepairRepository.GetAsync(id);
                _mapper.Map(dto, existing);
                existing.Id = id;
                existing.UpdatedAt = DateTime.UtcNow;
                await _adoUow.RepairRepository.ReplaceAsync(existing);
                return _mapper.Map<RepairResponceDTO>(existing);
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
                var existing = await _efUow.EFPRepairRepository.GetByIdAsync(id);
                _mapper.Map(dto, existing);
                existing.Id = id;
                existing.UpdatedAt = DateTime.UtcNow;
                await _efUow.EFPRepairRepository.UpdateAsync(existing);
                await _efUow.CompleteAsync(cancellationToken);
                return _mapper.Map<RepairResponceDTO>(existing);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Видалення ремонту
    /// </summary>
    public async Task<bool> DeleteRepairAsync(
        int id,
        DataAccessProvider provider,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Видалення ремонту {Id} через {Provider}", id, provider);

        if (provider == DataAccessProvider.Dapper)
        {
            try
            {
                await _adoUow.RepairRepository.GetAsync(id);
                await _adoUow.RepairRepository.DeleteAsync(id);
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
                await _efUow.EFPRepairRepository.DeleteByIdAsync(id);
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

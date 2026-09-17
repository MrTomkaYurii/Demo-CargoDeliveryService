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
/// Менеджер техоглядів. Підтримує виклики через EF Core та Dapper.
/// </summary>
public class InspectationManager : IInspectationManager
{
    private readonly ILogger<InspectationManager> _logger;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _adoUow;
    private readonly IEFUnitOfWork _efUow;

    public InspectationManager(
        ILogger<InspectationManager> logger,
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
    /// Отримання всіх техоглядів
    /// </summary>
    public async Task<IEnumerable<InspectionResponceDTO>> GetAllInspectionsAsync(
        DataAccessProvider provider,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Отримання всіх техоглядів через {Provider}", provider);

        if (provider == DataAccessProvider.Dapper)
        {
            var inspections = await _adoUow.InspectionRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<InspectionResponceDTO>>(inspections);
        }
        else
        {
            var inspections = await _efUow.EFInspectionRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<InspectionResponceDTO>>(inspections);
        }
    }

    /// <summary>
    /// Отримання техогляду за ідентифікатором
    /// </summary>
    public async Task<InspectionResponceDTO?> GetInspectionByIdAsync(
        int id,
        DataAccessProvider provider,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Отримання техогляду {Id} через {Provider}", id, provider);

        if (provider == DataAccessProvider.Dapper)
        {
            try
            {
                var inspection = await _adoUow.InspectionRepository.GetAsync(id);
                return _mapper.Map<InspectionResponceDTO>(inspection);
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
                var inspection = await _efUow.EFInspectionRepository.GetCompleteEntityAsync(id);
                return inspection != null ? _mapper.Map<InspectionResponceDTO>(inspection) : null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Отримання списку техоглядів для конкретної вантажівки
    /// </summary>
    public async Task<IEnumerable<InspectionResponceDTO>> GetInspectionsByTruckIdAsync(
        int truckId,
        DataAccessProvider provider,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Отримання техоглядів для вантажівки {TruckId} через {Provider}", truckId, provider);

        if (provider == DataAccessProvider.Dapper)
        {
            var inspections = await _adoUow.InspectionRepository.GetInspectionsByTruckIdAsync(truckId);
            return _mapper.Map<IEnumerable<InspectionResponceDTO>>(inspections);
        }
        else
        {
            var inspections = await _efUow.EFInspectionRepository.GetInspectionsByTruckIdAsync(truckId);
            return _mapper.Map<IEnumerable<InspectionResponceDTO>>(inspections);
        }
    }

    /// <summary>
    /// Створення нового техогляду
    /// </summary>
    public async Task<InspectionResponceDTO> CreateInspectionAsync(
        InspectionReqDTO dto,
        DataAccessProvider provider,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Створення техогляду через {Provider}", provider);

        if (provider == DataAccessProvider.Dapper)
        {
            var entity = _mapper.Map<Inspection>(dto);
            entity.CreatedAt = DateTime.UtcNow;
            var newId = await _adoUow.InspectionRepository.AddAsync(entity);
            entity.Id = newId;
            return _mapper.Map<InspectionResponceDTO>(entity);
        }
        else
        {
            var entity = _mapper.Map<EFInspection>(dto);
            entity.CreatedAt = DateTime.UtcNow;
            var added = await _efUow.EFInspectionRepository.AddAsync(entity);
            await _efUow.CompleteAsync(cancellationToken);
            return _mapper.Map<InspectionResponceDTO>(added);
        }
    }

    /// <summary>
    /// Оновлення техогляду
    /// </summary>
    public async Task<InspectionResponceDTO?> UpdateInspectionAsync(
        int id,
        InspectionReqDTO dto,
        DataAccessProvider provider,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Оновлення техогляду {Id} через {Provider}", id, provider);

        if (provider == DataAccessProvider.Dapper)
        {
            try
            {
                var existing = await _adoUow.InspectionRepository.GetAsync(id);
                _mapper.Map(dto, existing);
                existing.Id = id;
                existing.UpdatedAt = DateTime.UtcNow;
                await _adoUow.InspectionRepository.ReplaceAsync(existing);
                return _mapper.Map<InspectionResponceDTO>(existing);
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
                var existing = await _efUow.EFInspectionRepository.GetByIdAsync(id);
                _mapper.Map(dto, existing);
                existing.Id = id;
                existing.UpdatedAt = DateTime.UtcNow;
                await _efUow.EFInspectionRepository.UpdateAsync(existing);
                await _efUow.CompleteAsync(cancellationToken);
                return _mapper.Map<InspectionResponceDTO>(existing);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Видалення техогляду
    /// </summary>
    public async Task<bool> DeleteInspectionAsync(
        int id,
        DataAccessProvider provider,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Видалення техогляду {Id} через {Provider}", id, provider);

        if (provider == DataAccessProvider.Dapper)
        {
            try
            {
                await _adoUow.InspectionRepository.GetAsync(id);
                await _adoUow.InspectionRepository.DeleteAsync(id);
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
                await _efUow.EFInspectionRepository.DeleteByIdAsync(id);
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

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
/// Менеджер компаній. Підтримує перемикання між EF Core та Dapper.
/// </summary>
public class CompanyManager : ICompanyManager
{
    private readonly ILogger<CompanyManager> _logger;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _adoUow;
    private readonly IEFUnitOfWork _efUow;

    public CompanyManager(
        ILogger<CompanyManager> logger,
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
    /// Отримання компанії за ідентифікатором (сумісність)
    /// </summary>
    public async Task<CompanyResponceDTO?> GetCompanyById(int id)
    {
        return await GetCompanyByIdAsync(id, DataAccessProvider.EntityFramework);
    }

    /// <summary>
    /// Отримання всіх компаній
    /// </summary>
    public async Task<IEnumerable<CompanyResponceDTO>> GetAllCompaniesAsync(
        DataAccessProvider provider,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Отримання всіх компаній через {Provider}", provider);

        if (provider == DataAccessProvider.Dapper)
        {
            var companies = await _adoUow.CompanyRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<CompanyResponceDTO>>(companies);
        }
        else
        {
            var companies = await _efUow.EFCompanyRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<CompanyResponceDTO>>(companies);
        }
    }

    /// <summary>
    /// Отримання компанії за ідентифікатором
    /// </summary>
    public async Task<CompanyResponceDTO?> GetCompanyByIdAsync(
        int id,
        DataAccessProvider provider,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Отримання компанії {Id} через {Provider}", id, provider);

        if (provider == DataAccessProvider.Dapper)
        {
            try
            {
                var company = await _adoUow.CompanyRepository.GetAsync(id);
                return _mapper.Map<CompanyResponceDTO>(company);
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
                var company = await _efUow.EFCompanyRepository.GetByIdAsync(id);
                return _mapper.Map<CompanyResponceDTO>(company);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Отримання компанії разом зі списком закріплених водіїв
    /// </summary>
    public async Task<CompanyWithDriversResponseDTO?> GetCompanyWithDriversAsync(
        int id,
        DataAccessProvider provider,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Отримання компанії {Id} з водіями через {Provider}", id, provider);

        if (provider == DataAccessProvider.Dapper)
        {
            var company = await _adoUow.CompanyRepository.GetCompanyWithDriversAsync(id);
            return company != null ? _mapper.Map<CompanyWithDriversResponseDTO>(company) : null;
        }
        else
        {
            var company = await _efUow.EFCompanyRepository.GetCompanyWithDriversAsync(id);
            return company != null ? _mapper.Map<CompanyWithDriversResponseDTO>(company) : null;
        }
    }

    /// <summary>
    /// Створення нової компанії
    /// </summary>
    public async Task<CompanyResponceDTO> CreateCompanyAsync(
        CompanyReqDTO dto,
        DataAccessProvider provider,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Створення компанії {Name} через {Provider}", dto.CompanyName, provider);

        if (provider == DataAccessProvider.Dapper)
        {
            var entity = _mapper.Map<Company>(dto);
            entity.CreatedAt = DateTime.UtcNow;
            var newId = await _adoUow.CompanyRepository.AddAsync(entity);
            entity.Id = newId;
            return _mapper.Map<CompanyResponceDTO>(entity);
        }
        else
        {
            var entity = _mapper.Map<EFCompany>(dto);
            entity.CreatedAt = DateTime.UtcNow;
            var added = await _efUow.EFCompanyRepository.AddAsync(entity);
            await _efUow.CompleteAsync(cancellationToken);
            return _mapper.Map<CompanyResponceDTO>(added);
        }
    }

    /// <summary>
    /// Оновлення існуючої компанії
    /// </summary>
    public async Task<CompanyResponceDTO?> UpdateCompanyAsync(
        int id,
        CompanyReqDTO dto,
        DataAccessProvider provider,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Оновлення компанії {Id} через {Provider}", id, provider);

        if (provider == DataAccessProvider.Dapper)
        {
            try
            {
                var existing = await _adoUow.CompanyRepository.GetAsync(id);
                _mapper.Map(dto, existing);
                existing.Id = id;
                existing.UpdatedAt = DateTime.UtcNow;
                await _adoUow.CompanyRepository.ReplaceAsync(existing);
                return _mapper.Map<CompanyResponceDTO>(existing);
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
                var existing = await _efUow.EFCompanyRepository.GetByIdAsync(id);
                _mapper.Map(dto, existing);
                existing.Id = id;
                existing.UpdatedAt = DateTime.UtcNow;
                await _efUow.EFCompanyRepository.UpdateAsync(existing);
                await _efUow.CompleteAsync(cancellationToken);
                return _mapper.Map<CompanyResponceDTO>(existing);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Видалення компанії
    /// </summary>
    public async Task<bool> DeleteCompanyAsync(
        int id,
        DataAccessProvider provider,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Видалення компанії {Id} через {Provider}", id, provider);

        if (provider == DataAccessProvider.Dapper)
        {
            try
            {
                await _adoUow.CompanyRepository.GetAsync(id);
                await _adoUow.CompanyRepository.DeleteAsync(id);
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
                await _efUow.EFCompanyRepository.DeleteByIdAsync(id);
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

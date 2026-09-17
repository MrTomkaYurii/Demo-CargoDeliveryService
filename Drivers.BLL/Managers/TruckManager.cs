using System.Diagnostics;
using AutoMapper;
using Drivers.BLL.DTOs.Responses;
using Drivers.BLL.Managers.Contracts;
using Drivers.BLL.Models;
using Drivers.DAL.ADO.UOW.Contracts;
using Drivers.DAL.EF.UOW.Contracts;
using Microsoft.Extensions.Logging;

namespace Drivers.BLL.Managers;

/// <summary>
/// Менеджер автопарку (вантажівок).
/// Забезпечує доступ до даних автопарку та зведену аналітику обслуговування.
/// </summary>
public class TruckManager : ITruckManager
{
    private readonly ILogger<TruckManager> _logger;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _adoUow;
    private readonly IEFUnitOfWork _efUow;

    public TruckManager(
        ILogger<TruckManager> logger,
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
    /// Отримання списку всіх вантажівок через обраний провайдер
    /// </summary>
    public async Task<IEnumerable<TruckResponceDTO>> GetAllTrucks(
        DataAccessProvider provider,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Запит усіх вантажівок через провайдер {Provider}", provider);

        if (provider == DataAccessProvider.Dapper)
        {
            var trucks = await _adoUow.TruckRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<TruckResponceDTO>>(trucks);
        }
        else
        {
            var trucks = await _efUow.EFPTruckRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<TruckResponceDTO>>(trucks);
        }
    }

    /// <summary>
    /// Отримання зведеної аналітики обслуговування конкретної вантажівки
    /// </summary>
    public async Task<TruckMaintenanceSummaryResponseDTO?> GetTruckMaintenanceSummaryAsync(
        int truckId,
        DataAccessProvider provider,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Отримання аналітики обслуговування вантажівки {TruckId} через {Provider}", truckId, provider);

        if (provider == DataAccessProvider.Dapper)
        {
            var summary = await _adoUow.TruckRepository.GetTruckMaintenanceSummaryAsync(truckId);
            return summary != null ? _mapper.Map<TruckMaintenanceSummaryResponseDTO>(summary) : null;
        }
        else
        {
            var summary = await _efUow.EFPTruckRepository.GetTruckMaintenanceSummaryAsync(truckId);
            return summary != null ? _mapper.Map<TruckMaintenanceSummaryResponseDTO>(summary) : null;
        }
    }

    /// <summary>
    /// Порівняння швидкодії запиту аналітики обслуговування вантажівки між EF Core та Dapper
    /// </summary>
    public async Task<QueryComparisonResultDTO<TruckMaintenanceSummaryResponseDTO>> CompareTruckMaintenancePerformanceAsync(
        int truckId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Запуск порівняння швидкодії для вантажівки {TruckId}", truckId);

        // Замір EF Core
        var swEf = Stopwatch.StartNew();
        var efResult = await _efUow.EFPTruckRepository.GetTruckMaintenanceSummaryAsync(truckId);
        swEf.Stop();
        var efTime = swEf.Elapsed.TotalMilliseconds;

        // Замір Dapper
        var swDapper = Stopwatch.StartNew();
        var dapperResult = await _adoUow.TruckRepository.GetTruckMaintenanceSummaryAsync(truckId);
        swDapper.Stop();
        var dapperTime = swDapper.Elapsed.TotalMilliseconds;

        var mappedResult = efResult != null ? _mapper.Map<TruckMaintenanceSummaryResponseDTO>(efResult)
            : (dapperResult != null ? _mapper.Map<TruckMaintenanceSummaryResponseDTO>(dapperResult) : null);

        var faster = dapperTime <= efTime ? "Dapper" : "Entity Framework Core";
        var speedup = efTime > 0 && dapperTime > 0
            ? Math.Round(Math.Max(efTime, dapperTime) / Math.Min(efTime, dapperTime), 2)
            : 1.0;

        var match = (efResult == null && dapperResult == null) ||
                    (efResult != null && dapperResult != null &&
                     efResult.TotalMaintenanceCost == dapperResult.TotalMaintenanceCost &&
                     efResult.TotalRepairsCost == dapperResult.TotalRepairsCost &&
                     efResult.TotalInspectionsCount == dapperResult.TotalInspectionsCount);

        return new QueryComparisonResultDTO<TruckMaintenanceSummaryResponseDTO>
        {
            QueryName = "GetTruckMaintenanceSummary",
            Result = mappedResult,
            EntityFrameworkExecutionTimeMs = Math.Round(efTime, 3),
            DapperExecutionTimeMs = Math.Round(dapperTime, 3),
            FasterProvider = faster,
            SpeedupFactor = speedup,
            DataConsistent = match,
            Notes = match ? "Результати вибірок повністю ідентичні." : "Виявлено розбіжність у повернутих даних."
        };
    }
}

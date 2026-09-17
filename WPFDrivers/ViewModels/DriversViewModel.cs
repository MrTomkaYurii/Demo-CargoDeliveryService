using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Drivers.BLL.DTOs.Responses;
using Drivers.BLL.Managers.Contracts;
using Drivers.BLL.Models;
using Microsoft.Extensions.DependencyInjection;
using WPFDrivers.Commands;
using WPFDrivers.Models;
using WPFDrivers.Services;

namespace WPFDrivers.ViewModels;

/// <summary>
/// ViewModel для перегляду інформації про водіїв у WPF-клієнті.
/// Підтримує:
/// 1. Подвійний режим архітектури: Прямий BLL (In-Process) або REST Web API (Distributed Client-Server).
/// 2. Вибір провайдера доступу до даних: Entity Framework Core або ADO.NET + Dapper.
/// 3. Інтерактивний бенчмаркінг швидкодії запитів з наочним виведенням результатів.
/// 4. Аналітичні вибірки (звіт витрат, сповіщення закінчення ліцензій).
/// </summary>
public class DriversViewModel : ViewModelBase
{
    private readonly IServiceProvider _serviceProvider;

    private int _driverId = 1;
    private ArchitectureMode _selectedArchitecture = ArchitectureMode.DirectBll;
    private DataAccessProvider _selectedProvider = DataAccessProvider.EntityFramework;
    private int _selectedTabIndex = 0;
    private bool _isLoading;
    private string _statusMessage = "Готовий до роботи. Оберіть режим та натисніть запит.";
    private double _lastExecutionTimeMs;

    private FullDriverResponceDTO? _driverInfo;
    private QueryComparisonResultDTO<FullDriverResponceDTO>? _benchmarkResult;
    private DriverExpenseSummaryResponseDTO? _expenseSummary;
    private ObservableCollection<DriverLicenseAlertResponseDTO> _expiringLicenses = new();

    public DriversViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        GetDriverCommand = new AsyncRelayCommand(async () => await LoadDriverAsync(), () => !IsLoading);
        RunBenchmarkCommand = new AsyncRelayCommand(async () => await RunBenchmarkAsync(), () => !IsLoading);
        GetExpenseSummaryCommand = new AsyncRelayCommand(async () => await LoadExpenseSummaryAsync(), () => !IsLoading);
        GetExpiringLicensesCommand = new AsyncRelayCommand(async () => await LoadExpiringLicensesAsync(), () => !IsLoading);
    }

    #region Properties

    public int DriverId
    {
        get => _driverId;
        set => OnPropertyChanged(ref _driverId, value);
    }

    public ArchitectureMode SelectedArchitecture
    {
        get => _selectedArchitecture;
        set
        {
            if (OnPropertyChanged(ref _selectedArchitecture, value))
            {
                OnPropertyChanged(nameof(IsDirectMode));
                OnPropertyChanged(nameof(IsApiMode));
                StatusMessage = $"Змінено режим архітектури на: {(value == ArchitectureMode.DirectBll ? "Прямий BLL (In-Process)" : "Віддалений REST Web API")}";
            }
        }
    }

    public bool IsDirectMode
    {
        get => SelectedArchitecture == ArchitectureMode.DirectBll;
        set { if (value) SelectedArchitecture = ArchitectureMode.DirectBll; }
    }

    public bool IsApiMode
    {
        get => SelectedArchitecture == ArchitectureMode.WebApi;
        set { if (value) SelectedArchitecture = ArchitectureMode.WebApi; }
    }

    public DataAccessProvider SelectedProvider
    {
        get => _selectedProvider;
        set
        {
            if (OnPropertyChanged(ref _selectedProvider, value))
            {
                OnPropertyChanged(nameof(IsEfProvider));
                OnPropertyChanged(nameof(IsDapperProvider));
                StatusMessage = $"Змінено ORM провайдер на: {value}";
            }
        }
    }

    public bool IsEfProvider
    {
        get => SelectedProvider == DataAccessProvider.EntityFramework;
        set { if (value) SelectedProvider = DataAccessProvider.EntityFramework; }
    }

    public bool IsDapperProvider
    {
        get => SelectedProvider == DataAccessProvider.Dapper;
        set { if (value) SelectedProvider = DataAccessProvider.Dapper; }
    }

    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set => OnPropertyChanged(ref _selectedTabIndex, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => OnPropertyChanged(ref _isLoading, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => OnPropertyChanged(ref _statusMessage, value);
    }

    public double LastExecutionTimeMs
    {
        get => _lastExecutionTimeMs;
        set => OnPropertyChanged(ref _lastExecutionTimeMs, value);
    }

    public FullDriverResponceDTO? DriverInfo
    {
        get => _driverInfo;
        set => OnPropertyChanged(ref _driverInfo, value);
    }

    public QueryComparisonResultDTO<FullDriverResponceDTO>? BenchmarkResult
    {
        get => _benchmarkResult;
        set => OnPropertyChanged(ref _benchmarkResult, value);
    }

    public DriverExpenseSummaryResponseDTO? ExpenseSummary
    {
        get => _expenseSummary;
        set => OnPropertyChanged(ref _expenseSummary, value);
    }

    public ObservableCollection<DriverLicenseAlertResponseDTO> ExpiringLicenses
    {
        get => _expiringLicenses;
        set => OnPropertyChanged(ref _expiringLicenses, value);
    }

    #endregion

    #region Commands

    public ICommand GetDriverCommand { get; }
    public ICommand RunBenchmarkCommand { get; }
    public ICommand GetExpenseSummaryCommand { get; }
    public ICommand GetExpiringLicensesCommand { get; }

    #endregion

    #region Action Methods

    /// <summary>
    /// Завантаження даних водія відповідно до обраного режиму (Direct BLL або Web API) та провайдера
    /// </summary>
    public async Task LoadDriverAsync()
    {
        IsLoading = true;
        StatusMessage = $"Запит даних водія #{DriverId} через {GetModeDescription()}...";
        var sw = Stopwatch.StartNew();

        try
        {
            if (SelectedArchitecture == ArchitectureMode.DirectBll)
            {
                using var scope = _serviceProvider.CreateScope();
                var manager = scope.ServiceProvider.GetRequiredService<IDriversManager>();
                DriverInfo = await manager.GetFullInfoAboutDriver(DriverId, SelectedProvider, CancellationToken.None);
            }
            else
            {
                using var scope = _serviceProvider.CreateScope();
                var apiClient = scope.ServiceProvider.GetRequiredService<IDriversApiClient>();
                DriverInfo = await apiClient.GetFullDriverInfoAsync(DriverId, SelectedProvider, CancellationToken.None);
            }

            sw.Stop();
            LastExecutionTimeMs = sw.Elapsed.TotalMilliseconds;
            StatusMessage = $"Успішно завантажено водія #{DriverId}: {DriverInfo.FirstName} {DriverInfo.LastName} за {LastExecutionTimeMs:F1} мс ({SelectedArchitecture}, {SelectedProvider})";
            SelectedTabIndex = 0; // Перемикаємося на вкладку профілю
        }
        catch (Exception ex)
        {
            sw.Stop();
            StatusMessage = $"Помилка завантаження: {ex.Message}";
            MessageBox.Show(ex.Message, "Помилка виконання запиту", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Запуск прямого порівняльного бенчмаркінгу (EF Core проти Dapper)
    /// </summary>
    public async Task RunBenchmarkAsync()
    {
        IsLoading = true;
        StatusMessage = $"Запуск бенчмаркінгу EF Core vs Dapper для водія #{DriverId} через {GetModeDescription()}...";
        var sw = Stopwatch.StartNew();

        try
        {
            if (SelectedArchitecture == ArchitectureMode.DirectBll)
            {
                using var scope = _serviceProvider.CreateScope();
                var manager = scope.ServiceProvider.GetRequiredService<IDriversManager>();
                BenchmarkResult = await manager.CompareDriverQueryPerformanceAsync(DriverId, CancellationToken.None);
            }
            else
            {
                using var scope = _serviceProvider.CreateScope();
                var apiClient = scope.ServiceProvider.GetRequiredService<IDriversApiClient>();
                BenchmarkResult = await apiClient.CompareDriverProvidersAsync(DriverId, CancellationToken.None);
            }

            sw.Stop();
            LastExecutionTimeMs = sw.Elapsed.TotalMilliseconds;
            if (BenchmarkResult?.Result != null)
            {
                DriverInfo = BenchmarkResult.Result;
            }

            StatusMessage = $"Бенчмарк завершено! EF: {BenchmarkResult?.EntityFrameworkExecutionTimeMs:F2} мс | Dapper: {BenchmarkResult?.DapperExecutionTimeMs:F2} мс. Переможець: {BenchmarkResult?.FasterProvider} ({BenchmarkResult?.SpeedupFactor:F1}x)";
            SelectedTabIndex = 1; // Перемикаємося на вкладку бенчмаркінгу
        }
        catch (Exception ex)
        {
            sw.Stop();
            StatusMessage = $"Помилка бенчмаркінгу: {ex.Message}";
            MessageBox.Show(ex.Message, "Помилка бенчмаркінгу", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Завантаження фінансової аналітики витрат водія
    /// </summary>
    public async Task LoadExpenseSummaryAsync()
    {
        IsLoading = true;
        StatusMessage = $"Завантаження аналітики витрат водія #{DriverId} через {GetModeDescription()}...";
        var sw = Stopwatch.StartNew();

        try
        {
            if (SelectedArchitecture == ArchitectureMode.DirectBll)
            {
                using var scope = _serviceProvider.CreateScope();
                var manager = scope.ServiceProvider.GetRequiredService<IDriversManager>();
                ExpenseSummary = await manager.GetDriverExpenseSummaryAsync(DriverId, null, null, SelectedProvider, CancellationToken.None);
            }
            else
            {
                using var scope = _serviceProvider.CreateScope();
                var apiClient = scope.ServiceProvider.GetRequiredService<IDriversApiClient>();
                ExpenseSummary = await apiClient.GetDriverExpenseSummaryAsync(DriverId, null, null, SelectedProvider, CancellationToken.None);
            }

            sw.Stop();
            LastExecutionTimeMs = sw.Elapsed.TotalMilliseconds;
            StatusMessage = $"Звіт витрат завантажено за {LastExecutionTimeMs:F1} мс ({SelectedProvider}). Всього: ${ExpenseSummary.GrandTotal:N2}";
            SelectedTabIndex = 2; // Перемикаємося на вкладку аналітики
        }
        catch (Exception ex)
        {
            sw.Stop();
            StatusMessage = $"Помилка завантаження витрат: {ex.Message}";
            MessageBox.Show(ex.Message, "Помилка аналітики", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Завантаження списку водіїв з ліцензіями, що закінчуються
    /// </summary>
    public async Task LoadExpiringLicensesAsync()
    {
        IsLoading = true;
        StatusMessage = $"Перевірка закінчення терміну дії посвідчень через {GetModeDescription()}...";
        var sw = Stopwatch.StartNew();

        try
        {
            IEnumerable<DriverLicenseAlertResponseDTO> items;
            if (SelectedArchitecture == ArchitectureMode.DirectBll)
            {
                using var scope = _serviceProvider.CreateScope();
                var manager = scope.ServiceProvider.GetRequiredService<IDriversManager>();
                items = await manager.GetDriversWithExpiringLicenseAsync(30, SelectedProvider, CancellationToken.None);
            }
            else
            {
                using var scope = _serviceProvider.CreateScope();
                var apiClient = scope.ServiceProvider.GetRequiredService<IDriversApiClient>();
                items = await apiClient.GetExpiringLicensesAsync(30, SelectedProvider, CancellationToken.None);
            }

            sw.Stop();
            LastExecutionTimeMs = sw.Elapsed.TotalMilliseconds;
            ExpiringLicenses = new ObservableCollection<DriverLicenseAlertResponseDTO>(items);
            StatusMessage = $"Знайдено водіїв з терміном дії ліцензії що завершується: {ExpiringLicenses.Count} за {LastExecutionTimeMs:F1} мс";
            SelectedTabIndex = 2; // Перемикаємося на вкладку аналітики
        }
        catch (Exception ex)
        {
            sw.Stop();
            StatusMessage = $"Помилка перевірки посвідчень: {ex.Message}";
            MessageBox.Show(ex.Message, "Помилка ліцензій", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private string GetModeDescription()
    {
        return SelectedArchitecture == ArchitectureMode.DirectBll
            ? $"Direct BLL ({SelectedProvider})"
            : $"REST API ({SelectedProvider})";
    }

    #endregion
}

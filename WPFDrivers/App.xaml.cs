using System.Data;
using System.Windows;
using Drivers.BLL.Managers;
using Drivers.BLL.Managers.Contracts;
using Drivers.BLL.Mapping;
using Drivers.DAL.ADO.Repositories;
using Drivers.DAL.ADO.Repositories.Contracts;
using Drivers.DAL.ADO.UOW;
using Drivers.DAL.ADO.UOW.Contracts;
using Drivers.DAL.EF.Data;
using Drivers.DAL.EF.Entities;
using Drivers.DAL.EF.Helpers;
using Drivers.DAL.EF.Repositories;
using Drivers.DAL.EF.Repositories.Contracts;
using Drivers.DAL.EF.UOW;
using Drivers.DAL.EF.UOW.Contracts;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http;
using Serilog;
using WPFDrivers.Services;
using WPFDrivers.ViewModels;

namespace WPFDrivers;

/// <summary>
/// Interaction logic for App.xaml
/// Головна точка входу для десктопного клієнта WPF Drivers
/// </summary>
public partial class App : Application
{
    private IHost? _appHost;

    public App()
    {
        try
        {
            _appHost = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) =>
                {
                    var envName = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT") ?? "Development";
                    configurationBuilder.AddEnvironmentVariables();
                    configurationBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    configurationBuilder.AddJsonFile($"appsettings.{envName}.json", optional: true, reloadOnChange: true);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var connectionString = hostContext.Configuration.GetConnectionString("MSSQLConnection") 
                        ?? "Server=(localdb)\\mssqllocaldb;Database=DriversDb;Trusted_Connection=True;MultipleActiveResultSets=true";

                    // Connection/Transaction for ADO.NET / DAPPER
                    services.AddScoped((s) => new SqlConnection(connectionString));
                    services.AddScoped<IDbTransaction>(s =>
                    {
                        var conn = s.GetRequiredService<SqlConnection>();
                        if (conn.State != ConnectionState.Open)
                        {
                            conn.Open();
                        }
                        return conn.BeginTransaction();
                    });

                    // ADO.NET DAL
                    services.AddScoped<IDriverRepository, DriverRepository>();
                    services.AddScoped<ICompanyRepository, CompanyRepository>();
                    services.AddScoped<IPhotoRepository, PhotoRepository>();
                    services.AddScoped<IExpensesRepository, ExpensesRepository>();
                    services.AddScoped<IInspectionRepository, InspectionRepository>();
                    services.AddScoped<ITruckRepository, TruckRepository>();
                    services.AddScoped<IRepairRepository, RepairRepository>();
                    services.AddScoped<IUnitOfWork, UnitOfWork>();

                    // EF Core DAL
                    services.AddDbContext<DriversManagementContext>(options =>
                    {
                        options.UseSqlServer(connectionString);
                    });
                    services.AddScoped<IEFDriverRepository, EFDriverRepository>();
                    services.AddScoped<IEFCompanyRepository, EFCompanyRepository>();
                    services.AddScoped<IEFExpenseRepository, EFExpenseRepository>();
                    services.AddScoped<IEFInspectionRepository, EFInspectionRepository>();
                    services.AddScoped<IEFPhotoRepository, EFPhotoRepository>();
                    services.AddScoped<IEFRepairRepository, EFRepairRepository>();
                    services.AddScoped<IEFTruckRepository, EFTruckRepository>();
                    services.AddScoped<IEFUnitOfWork, EFUnitOfWork>();

                    // Helpers
                    services.AddScoped<ISortHelper<EFDriver>, SortHelper<EFDriver>>();

                    // AutoMapper
                    services.AddAutoMapper(typeof(BLLMappingProfile).Assembly);

                    // BLL Managers
                    services.AddScoped<IDriversManager, DriversManager>();
                    services.AddScoped<ICompanyManager, CompanyManager>();
                    services.AddScoped<IExpenseManager, ExpenseManager>();
                    services.AddScoped<IInspectationManager, InspectationManager>();
                    services.AddScoped<IPhotoManager, PhotoManager>();
                    services.AddScoped<IRepairManager, RepairManager>();
                    services.AddScoped<ITruckManager, TruckManager>();

                    // REST Web API Client (Distributed Architecture)
                    var apiBaseUrl = hostContext.Configuration["DriversApi:BaseUrl"] ?? "https://localhost:4002/";
                    services.AddHttpClient<IDriversApiClient, DriversApiClient>(client =>
                    {
                        client.BaseAddress = new Uri(apiBaseUrl);
                        client.Timeout = TimeSpan.FromSeconds(15);
                    }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    });

                    // ViewModels & Windows
                    services.AddTransient<MainViewModel>();
                    services.AddTransient<DriversViewModel>();
                    services.AddSingleton(sp => new MainWindow(sp.GetRequiredService<MainViewModel>()));
                })
                .UseSerilog((context, services, loggerConfiguration) =>
                    loggerConfiguration.ReadFrom.Configuration(context.Configuration))
                .Build();
        }
        catch (Exception ex)
        {
            if (ex.GetType().Name.Equals("StopTheHostException", StringComparison.Ordinal))
            {
                throw;
            }

            Log.Fatal(ex, "Помилка ініціалізації хоста WPFDrivers");
            Current.Shutdown();
        }
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        if (_appHost != null)
        {
            await _appHost.StartAsync();

            using var scope = _appHost.Services.CreateScope();
            var mainWindow = scope.ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            mainWindow.Show();
        }

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        try
        {
            if (_appHost != null)
            {
                await _appHost.StopAsync();
                _appHost.Dispose();
            }

            Log.CloseAndFlush();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Помилка при зупинці хоста WPFDrivers");
        }
        finally
        {
            base.OnExit(e);
        }
    }
}

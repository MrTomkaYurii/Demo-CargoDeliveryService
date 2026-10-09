namespace DriversManagement.Databases;

using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class MigrationHostedService<TDbContext> : IHostedService
    where TDbContext : DbContext
{
    private readonly ILogger<MigrationHostedService<TDbContext>> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public MigrationHostedService(IServiceScopeFactory scopeFactory, ILogger<MigrationHostedService<TDbContext>> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Applying migrations for {DbContext}", typeof(TDbContext).Name);

            await using var scope = _scopeFactory.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<TDbContext>();

            // ====================================================================================
            // 🗄️ СТРАТЕГІЯ ІНІЦІАЛІЗАЦІЇ СХЕМИ БД ПІД ЧАС СТАРТУ СЕРВІСУ
            // ------------------------------------------------------------------------------------
            // Aspire AppHost щоразу піднімає НОВИЙ контейнер SQL Server без томів даних
            // (.WithDataVolume() не використовується), тому на старті база порожня і її схему
            // потрібно створити автоматично:
            //   1. Якщо у збірці є EF Core міграції — застосовуємо їх (MigrateAsync). Це
            //      еталонний підхід: історія змін схеми зберігається в __EFMigrationsHistory.
            //   2. Якщо міграцій немає (наприклад, у CargoOrderingService) — створюємо схему
            //      напряму з поточної моделі (EnsureCreatedAsync). Без цього MigrateAsync
            //      створив би лише порожню БД без жодної таблиці.
            // ⚠️ Не змішуйте ці підходи на ОДНІЙ базі: EnsureCreated не веде історію міграцій,
            // і подальший MigrateAsync впаде з помилкою "There is already an object named ...".
            // Саме так виникають проблеми зі "старими" базами, створеними раніше іншим способом.
            // ====================================================================================
            if (context.Database.GetMigrations().Any())
            {
                await context.Database.MigrateAsync(cancellationToken);
            }
            else
            {
                await context.Database.EnsureCreatedAsync(cancellationToken);
            }

            _logger.LogInformation("Migrations complete for {DbContext}", typeof(TDbContext).Name);
        }
        catch (Exception ex) when (ex is SocketException)
        {
            _logger.LogError(ex, "Could not connect to the database. Please check the connection string and make sure the database is running.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while applying the database migrations.");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
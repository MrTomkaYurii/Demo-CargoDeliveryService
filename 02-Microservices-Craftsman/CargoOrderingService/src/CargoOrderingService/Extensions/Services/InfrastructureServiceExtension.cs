namespace CargoOrderingService.Extensions.Services;

using CargoOrderingService.Databases;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using HeimGuard;
using CargoOrderingService.Resources;
using CargoOrderingService.Services;
using CargoOrderingService.Resources.HangfireUtilities;
using Configurations;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.EntityFrameworkCore;

public static class ServiceRegistration
{
    public static void AddInfrastructure(this IServiceCollection services, IWebHostEnvironment env, IConfiguration configuration)
    {
        // DbContext -- Do Not Delete
        var connectionString = configuration.GetConnectionStringOptions().CargoOrderingService;
        if(string.IsNullOrWhiteSpace(connectionString))
        {
            // this makes local migrations easier to manage. feel free to refactor if desired.
            connectionString = env.IsDevelopment() 
                ? "Data Source=localhost,57192;Integrated Security=False;Database=dev_cargoorderingservice;User ID=SA;Password=#localDockerPassword#"
                : throw new Exception("The database connection string is not set.");
        }

        services.AddDbContext<OrderingContext>(options =>
            options.UseSqlServer(connectionString,
                builder => builder.MigrationsAssembly(typeof(OrderingContext).Assembly.FullName))
                            .UseSnakeCaseNamingConvention());

        services.AddHostedService<MigrationHostedService<OrderingContext>>();

        services.SetupHangfire(env);

        // ====================================================================================
        // 🔓 [AUTH-DISABLED] АВТЕНТИФІКАЦІЮ ВИМКНЕНО ДЛЯ НАВЧАЛЬНОГО СТЕНДУ ASPIRE
        // ------------------------------------------------------------------------------------
        // Оригінальний Craftsman-шаблон вимагає JWT від Keycloak (http://localhost:5000), який
        // у цьому навчальному стенді не запускається. Тому закоментовано:
        //   - реєстрацію JwtBearer (нижче);
        //   - app.UseAuthentication() у Program.cs;
        //   - атрибути [Authorize] у контролерах;
        //   - перевірки дозволів HeimGuard (_heimGuard.MustHavePermission) в обробниках.
        // Усі такі рядки позначені маркером "[AUTH-DISABLED]": щоб повернути авторизацію,
        // знайдіть маркер у рішенні та розкоментуйте рядки.
        // AddAuthorization та AddHeimGuard ЗАЛИШЕНО, бо IHeimGuardClient інжектується в обробники.
        // ====================================================================================
        // [AUTH-DISABLED] // Auth -- Do Not Delete
        // [AUTH-DISABLED] var authOptions = configuration.GetAuthOptions();
        // [AUTH-DISABLED] if (!env.IsEnvironment(Consts.Testing.FunctionalTestingEnvName))
        // [AUTH-DISABLED] {
        // [AUTH-DISABLED] services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        // [AUTH-DISABLED] .AddJwtBearer(options =>
        // [AUTH-DISABLED] {
        // [AUTH-DISABLED] options.Authority = authOptions.Authority;
        // [AUTH-DISABLED] options.Audience = authOptions.Audience;
        // [AUTH-DISABLED] options.RequireHttpsMetadata = !env.IsDevelopment();
        // [AUTH-DISABLED] });
        // [AUTH-DISABLED] }

        services.AddAuthorization(options =>
        {
        });

        services.AddHeimGuard<UserPolicyHandler>()
            .MapAuthorizationPolicies()
            .AutomaticallyCheckPermissions();
    }
}
    
public static class HangfireConfig
{
    public static void SetupHangfire(this IServiceCollection services, IWebHostEnvironment env)
    {
        services.AddScoped<IJobContextAccessor, JobContextAccessor>();
        services.AddScoped<IJobWithUserContext, JobWithUserContext>();
        // if you want tags with sql server
        // var tagOptions = new TagsOptions() { TagsListStyle = TagsListStyle.Dropdown };
        
        // var hangfireConfig = new MemoryStorageOptions() { };
        services.AddHangfire(config =>
        {
            config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseMemoryStorage()
                .UseColouredConsoleLogProvider()
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                // if you want tags with sql server
                // .UseTagsWithSql(tagOptions, hangfireConfig)
                .UseActivator(new JobWithUserContextActivator(services.BuildServiceProvider()
                    .GetRequiredService<IServiceScopeFactory>()));
        });
        services.AddHangfireServer(options =>
        {
            options.WorkerCount = 10;
            options.ServerName = $"PeakLims-{env.EnvironmentName}";

            if (Consts.HangfireQueues.List().Length > 0)
            {
                options.Queues = Consts.HangfireQueues.List();
            }
        });

    }
}

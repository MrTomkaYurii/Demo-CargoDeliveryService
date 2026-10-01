namespace DriversBlogManagement.FunctionalTests;

using DriversBlogManagement.Databases;
using DriversBlogManagement;
using DriversBlogManagement.Domain.Roles;
using DriversBlogManagement.Domain.Users;
using DriversBlogManagement.SharedTestHelpers.Fakes.User;
using AutoBogus;
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;
 
[Collection(nameof(TestBase))]
public class TestBase : IDisposable
{
    private static IServiceScopeFactory _scopeFactory;
    protected static HttpClient FactoryClient  { get; private set; }

    public TestBase()
    {
        var factory = new TestingWebApplicationFactory();
        _scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();
        FactoryClient = factory.CreateClient(new WebApplicationFactoryClientOptions());

        AutoFaker.Configure(builder =>
        {
            // configure global autobogus settings here
            builder.WithDateTimeKind(DateTimeKind.Utc)
                .WithRecursiveDepth(3)
                .WithTreeDepth(1)
                .WithRepeatCount(1);
        });
        
        // seed root user so tests won't always have user as super admin
        AddNewSuperAdmin().Wait();
    }
    
    public void Dispose()
    {
        FactoryClient.Dispose();
    }

    public static async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        using var scope = _scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetService<ISender>();
        return await mediator.Send(request);
    }

    public static async Task<TEntity> FindAsync<TEntity>(params object[] keyValues)
        where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetService<BlogManagementContext>();
        return await context.FindAsync<TEntity>(keyValues);
    }

    public static async Task AddAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetService<BlogManagementContext>();
        context.Add(entity);
        await context.SaveChangesAsync();
    }

    public static async Task ExecuteScopeAsync(Func<IServiceProvider, Task> action)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BlogManagementContext>();
        await action(scope.ServiceProvider);
    }

    public static async Task<T> ExecuteScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BlogManagementContext>();
        return await action(scope.ServiceProvider);
    }

    public static Task ExecuteDbContextAsync(Func<BlogManagementContext, Task> action)
        => ExecuteScopeAsync(sp => action(sp.GetService<BlogManagementContext>()));

    public static Task ExecuteDbContextAsync(Func<BlogManagementContext, ValueTask> action)
        => ExecuteScopeAsync(sp => action(sp.GetService<BlogManagementContext>()).AsTask());

    public static Task ExecuteDbContextAsync(Func<BlogManagementContext, IMediator, Task> action)
        => ExecuteScopeAsync(sp => action(sp.GetService<BlogManagementContext>(), sp.GetService<IMediator>()));

    public static Task<T> ExecuteDbContextAsync<T>(Func<BlogManagementContext, Task<T>> action)
        => ExecuteScopeAsync(sp => action(sp.GetService<BlogManagementContext>()));

    public static Task<T> ExecuteDbContextAsync<T>(Func<BlogManagementContext, ValueTask<T>> action)
        => ExecuteScopeAsync(sp => action(sp.GetService<BlogManagementContext>()).AsTask());

    public static Task<T> ExecuteDbContextAsync<T>(Func<BlogManagementContext, IMediator, Task<T>> action)
        => ExecuteScopeAsync(sp => action(sp.GetService<BlogManagementContext>(), sp.GetService<IMediator>()));

    public static Task<int> InsertAsync<T>(params T[] entities) where T : class
    {
        return ExecuteDbContextAsync(db =>
        {
            foreach (var entity in entities)
            {
                db.Set<T>().Add(entity);
            }
            return db.SaveChangesAsync();
        });
    }

    public static async Task<User> AddNewSuperAdmin()
    {
        var user = new FakeUserBuilder().Build();
        user.AddRole(Role.SuperAdmin());
        await InsertAsync(user);
        return user;
    }

    public static async Task<User> AddNewUser(List<Role> roles)
    {
        var user = new FakeUserBuilder().Build();
        foreach (var role in roles)
            user.AddRole(role);
        
        await InsertAsync(user);
        return user;
    }

    public static async Task<User> AddNewUser(params Role[] roles)
        => await AddNewUser(roles.ToList());
}
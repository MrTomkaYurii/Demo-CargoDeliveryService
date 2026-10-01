using AuthorizationModuleAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthorizationModuleAPI.Data;

public class DataContext : DbContext
{
    public DbSet<Account> Accounts { get; set; }

    private readonly IConfiguration Configuration;

    public DataContext(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        // connect to sqlite database
        options.UseSqlServer(Configuration.GetConnectionString("WebApiJWTDatabase"));
    }
}
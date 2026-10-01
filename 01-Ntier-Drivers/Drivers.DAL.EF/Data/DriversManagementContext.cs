using Drivers.DAL.EF.Entities;
using Microsoft.EntityFrameworkCore;

namespace Drivers.DAL.EF.Data
{
    public partial class DriversManagementContext : DbContext
    {
        public DriversManagementContext()
        {
        }

        public DriversManagementContext(DbContextOptions<DriversManagementContext> options)
            : base(options)
        {
        }

        public virtual DbSet<EFCompany> Companies { get; set; } = null!;
        public virtual DbSet<EFDriver> Drivers { get; set; } = null!;
        public virtual DbSet<EFExpense> Expenses { get; set; } = null!;
        public virtual DbSet<EFInspection> Inspections { get; set; } = null!;
        public virtual DbSet<EFPhoto> Photos { get; set; } = null!;
        public virtual DbSet<EFRepair> Repairs { get; set; } = null!;
        public virtual DbSet<EFTruck> Trucks { get; set; } = null!;
    }
}

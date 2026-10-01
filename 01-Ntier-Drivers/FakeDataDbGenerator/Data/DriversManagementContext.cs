using System;
using System.Collections.Generic;
using FakeDataDriverDbGenerator.Configurations;
using FakeDataDriverDbGenerator.Entities;
using FakeDataDriverDbGenerator.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

namespace FakeDataDriverDbGenerator.Data
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

        public virtual DbSet<Company> Companies { get; set; } = null!;
        public virtual DbSet<Driver> Drivers { get; set; } = null!;
        public virtual DbSet<Expense> Expenses { get; set; } = null!;
        public virtual DbSet<Inspection> Inspections { get; set; } = null!;
        public virtual DbSet<Photo> Photos { get; set; } = null!;
        public virtual DbSet<Repair> Repairs { get; set; } = null!;
        public virtual DbSet<Truck> Trucks { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json")
                    .Build();

                string? connectionString = configuration.GetConnectionString("DefaultConnection");
                if (connectionString != null)
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new CompanyConfiguration());
            modelBuilder.ApplyConfiguration(new DriverConfiguration());
            modelBuilder.ApplyConfiguration(new ExpenseConfiguration());
            modelBuilder.ApplyConfiguration(new InspectionConfiguration());
            modelBuilder.ApplyConfiguration(new PhotoConfiguration());
            modelBuilder.ApplyConfiguration(new RepairConfiguration());
            modelBuilder.ApplyConfiguration(new TruckConfiguration());

            var databaseSeeder = new DriversManagementDatabaseSeeder();

            modelBuilder.Entity<Company>().HasData(databaseSeeder.Companies);
            modelBuilder.Entity<Photo>().HasData(databaseSeeder.Photos);
            modelBuilder.Entity<Driver>().HasData(databaseSeeder.Drivers);
            
            modelBuilder.Entity<Inspection>().HasData(databaseSeeder.Inspections);
            modelBuilder.Entity<Repair>().HasData(databaseSeeder.Repairs);
            modelBuilder.Entity<Truck>().HasData(databaseSeeder.Trucks);

            modelBuilder.Entity<Expense>().HasData(databaseSeeder.Expenses);
        }
    }
}
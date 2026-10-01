using FakeDataDriverDbGenerator.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeDataDriverDbGenerator.Configurations
{
    public class TruckConfiguration : IEntityTypeConfiguration<Truck>
    {
        public void Configure (EntityTypeBuilder<Truck> modelbuilder)
        {
            modelbuilder.HasKey(t => t.Id);
            modelbuilder.Property(t => t.Id).HasColumnName("Id");

            modelbuilder.Property(t => t.ChassisNumber).HasMaxLength(50);
            modelbuilder.Property(t => t.EngineNumber).HasMaxLength(50);
            modelbuilder.Property(t => t.FuelConsumption).HasColumnType("decimal(4, 2)");
            modelbuilder.Property(t => t.FuelType).HasMaxLength(50);
            modelbuilder.Property(t => t.InsuranceExpirationDate).HasColumnType("date");
            modelbuilder.Property(t => t.InsurancePolicyNumber).HasMaxLength(50);
            modelbuilder.Property(t => t.Model).HasMaxLength(50);
            modelbuilder.Property(t => t.RegistrationNumber).HasMaxLength(20);
            modelbuilder.Property(t => t.TruckNumber).HasMaxLength(20);
            modelbuilder.Property(t => t.Vin).HasMaxLength(50).HasColumnName("VIN");

            modelbuilder.Property(d => d.CreatedAt)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");
            modelbuilder.Property(d => d.UpdatedAt)
                .HasColumnType("datetime")
                .HasDefaultValueSql(null);
            modelbuilder.Property(d => d.DeletedAt)
                .HasColumnType("datetime")
                .HasDefaultValue(null);
        }
    }
}

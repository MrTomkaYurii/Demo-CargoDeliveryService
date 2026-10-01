using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FakeDataDriverDbGenerator.Entities;
using System.Reflection.Emit;

namespace FakeDataDriverDbGenerator.Configurations
{
    public class DriverConfiguration : IEntityTypeConfiguration<Driver>
    {
        public void Configure(EntityTypeBuilder<Driver> modelBuilder)
        {
            modelBuilder.HasKey(d => d.Id);
            modelBuilder.Property(d => d.Id).HasColumnName("Id");

            modelBuilder.Property(d => d.FirstName).HasMaxLength(50).IsRequired();
            modelBuilder.Property(d => d.LastName).HasMaxLength(50).IsRequired();
            modelBuilder.Property(d => d.MiddleName).HasMaxLength(50);
            modelBuilder.Property(d => d.Gender).HasMaxLength(10).IsRequired();
            modelBuilder.Property(d => d.Birthdate).HasColumnType("date").IsRequired();
            modelBuilder.Property(d => d.PlaceOfBirth).HasMaxLength(100);
            modelBuilder.Property(d => d.Nationality).HasMaxLength(50);
            modelBuilder.Property(d => d.MaritalStatus).HasMaxLength(20);
            modelBuilder.Property(d => d.IdentificationType).HasMaxLength(50);
            modelBuilder.Property(d => d.IdentificationNumber).HasMaxLength(50);
            modelBuilder.Property(d => d.IdentificationExpirationDate).HasColumnType("date");
            modelBuilder.Property(d => d.Address).HasMaxLength(100);
            modelBuilder.Property(d => d.Phone).HasMaxLength(20);
            modelBuilder.Property(d => d.Email).HasMaxLength(50);

            modelBuilder.Property(d => d.DriverLicenseNumber).HasMaxLength(20).IsRequired();
            modelBuilder.Property(d => d.DriverLicenseCategory).HasMaxLength(50);
            modelBuilder.Property(d => d.DriverLicenseIssuingDate).HasColumnType("date").IsRequired();
            modelBuilder.Property(d => d.DriverLicenseExpirationDate).HasColumnType("date").IsRequired();
            modelBuilder.Property(d => d.DriverLicenseIssuingAuthority).HasMaxLength(100);

            modelBuilder.Property(d => d.EmploymentStatus).HasMaxLength(50);
            modelBuilder.Property(d => d.EmploymentStartDate).HasColumnType("date");
            modelBuilder.Property(d => d.EmploymentEndDate).HasColumnType("date");

            modelBuilder.Property(d => d.CreatedAt)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");
            modelBuilder.Property(d => d.UpdatedAt)
                .HasColumnType("datetime")
                .HasDefaultValueSql(null);
            modelBuilder.Property(d => d.DeletedAt)
                .HasColumnType("datetime")
                .HasDefaultValue(null);


            modelBuilder.Property(d => d.PhotoId).HasColumnName("PhotoID");
            modelBuilder.Property(d => d.CompanyId).HasColumnName("CompanyID");

            modelBuilder.HasIndex(d => d.PhotoId).IsUnique();
            modelBuilder.HasIndex(d => d.CompanyId).IsUnique(); ;

            modelBuilder.HasOne(d => d.Company)
                .WithOne(c => c.Driver)
                .HasForeignKey<Driver>(d => d.CompanyId);

            modelBuilder.HasOne(d => d.Photo)
                .WithOne(p => p.Driver)
                .HasForeignKey<Driver>(d => d.PhotoId);
        }
    }
}


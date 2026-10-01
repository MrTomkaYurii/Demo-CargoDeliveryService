using FakeDataDriverDbGenerator.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace FakeDataDriverDbGenerator.Configurations
{
    public class CompanyConfiguration : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> modelbuilder)
        {
            modelbuilder.HasKey(c => c.Id);
            modelbuilder.Property(c => c.Id).HasColumnName("Id");

            modelbuilder.Property(c => c.CompanyName).HasMaxLength(100).IsRequired();
            modelbuilder.Property(c => c.Address).HasMaxLength(100);
            modelbuilder.Property(c => c.Phone).HasMaxLength(20);
            modelbuilder.Property(c => c.Email).HasMaxLength(50);
            modelbuilder.Property(c => c.ContactEmail).HasMaxLength(50);
            modelbuilder.Property(c => c.ContactPerson).HasMaxLength(100);
            modelbuilder.Property(c => c.ContactPhone).HasMaxLength(20);

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

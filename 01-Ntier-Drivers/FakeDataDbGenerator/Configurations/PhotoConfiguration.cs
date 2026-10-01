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
    public class PhotoConfiguration : IEntityTypeConfiguration<Photo>
    {
        public void Configure(EntityTypeBuilder<Photo> modelbuilder)
        {
            modelbuilder.HasKey(p => p.Id);
            modelbuilder.Property(p => p.Id).HasColumnName("Id");

            modelbuilder.Property(p => p.ContentType).HasMaxLength(50).IsRequired();
            modelbuilder.Property(p => p.FileName).HasMaxLength(255).IsRequired();

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

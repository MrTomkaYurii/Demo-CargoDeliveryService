using FakeDataDriverDbGenerator.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeDataDriverDbGenerator.Configurations
{
    public class InspectionConfiguration : IEntityTypeConfiguration<Inspection>
    {
        public void Configure(EntityTypeBuilder<Inspection> modelbuilder)
        {
            modelbuilder.HasKey(i => i.Id);
            modelbuilder.Property(i => i.Id).HasColumnName("Id");


            modelbuilder.Property(i => i.Description).HasMaxLength(500);
            modelbuilder.Property(i => i.InspectionDate).HasColumnType("date").IsRequired();
            modelbuilder.Property(i => i.Result).HasColumnType("bit").IsRequired();



            modelbuilder.Property(d => d.CreatedAt)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");
            modelbuilder.Property(d => d.UpdatedAt)
                .HasColumnType("datetime")
                .HasDefaultValueSql(null);
            modelbuilder.Property(d => d.DeletedAt)
                .HasColumnType("datetime")
                .HasDefaultValue(null);


            modelbuilder.Property(i => i.TruckId).HasColumnName("TruckID");
            modelbuilder.HasOne(i => i.Truck)
                .WithMany(t => t.Inspections)
                .HasForeignKey(i => i.TruckId);

        }
    }
}

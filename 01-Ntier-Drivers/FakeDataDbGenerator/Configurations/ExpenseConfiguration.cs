using FakeDataDriverDbGenerator.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FakeDataDriverDbGenerator.Configurations
{
    public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
    {
        public void Configure(EntityTypeBuilder<Expense> modelbuilder)
        {
            modelbuilder.HasKey(e => e.Id);
            modelbuilder.Property(e => e.Id).HasColumnName("Id");

            modelbuilder.Property(e => e.DriverPayment).HasColumnType("decimal(10, 2)").IsRequired();
            modelbuilder.Property(e => e.FuelCost).HasColumnType("decimal(10, 2)").IsRequired();
            modelbuilder.Property(e => e.MaintenanceCost).HasColumnType("decimal(10, 2)").IsRequired();
            modelbuilder.Property(e => e.Category).HasMaxLength(255).IsUnicode(false).IsRequired();
            modelbuilder.Property(e => e.Date).HasColumnType("date").IsRequired();
            modelbuilder.Property(e => e.Note).HasColumnType("text");

            modelbuilder.Property(d => d.CreatedAt)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");
            modelbuilder.Property(d => d.UpdatedAt)
                .HasColumnType("datetime")
                .HasDefaultValueSql(null);
            modelbuilder.Property(d => d.DeletedAt)
                .HasColumnType("datetime")
                .HasDefaultValue(null);

            modelbuilder.Property(e => e.DriverId).HasColumnName("DriverID");
            modelbuilder.Property(e => e.TruckId).HasColumnName("TruckID");
            modelbuilder.HasOne(e => e.Driver)
                        .WithMany(d => d.Expenses)
                        .HasForeignKey(e => e.DriverId)
                        .OnDelete(DeleteBehavior.ClientSetNull);
            modelbuilder.HasOne(e => e.Truck)
                        .WithMany(t => t.Expenses)
                        .HasForeignKey(e => e.TruckId)
                        .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}

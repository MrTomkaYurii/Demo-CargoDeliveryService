namespace CargoOrderingService.Databases.EntityConfigurations;

using CargoOrderingService.Domain.Drivers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class DriverConfiguration : IEntityTypeConfiguration<Driver>
{
    public void Configure(EntityTypeBuilder<Driver> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FirstName).HasMaxLength(100);
        builder.Property(x => x.LastName).HasMaxLength(100);
        builder.Property(x => x.DriverLicenseNumber).HasMaxLength(50);
        builder.Property(x => x.Phone).HasMaxLength(30);

        builder.HasMany(x => x.Orders)
            .WithOne(x => x.Driver);

        builder.HasMany(x => x.Deliveries)
            .WithOne(x => x.Driver);
    }
}

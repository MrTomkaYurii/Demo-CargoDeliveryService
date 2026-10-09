namespace DriversManagement.Databases.EntityConfigurations;

using DriversManagement.Domain.Companies;
using DriversManagement.Domain.Drivers;
using DriversManagement.Domain.Photos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class DriverConfiguration : IEntityTypeConfiguration<Driver>
{
    /// <summary>
    /// The database configuration for Drivers. 
    /// </summary>
    public void Configure(EntityTypeBuilder<Driver> builder)
    {
        // Relationship Marker -- Deleting or modifying this comment could cause incomplete relationship scaffolding
        // ⚠️ ВИПРАВЛЕНО: Craftsman згенерував зв'язок "один-до-одного" зі СПІЛЬНИМ первинним ключем
        // (HasForeignKey<Driver>(s => s.Id)), тобто Driver.Id мусив дорівнювати Id Company/Photo.
        // На практиці це унеможливлювало вставку: EF Core під час fix-up підміняв Id нового
        // запису (або ловив порушення FK). Тепер залежною стороною є Company та Photo, а зв'язок
        // тримає окрема опційна колонка driver_id (snake_case від тіньової властивості DriverId).
        builder.HasOne(x => x.Company)
            .WithOne(x => x.Driver)
            .HasForeignKey<Company>("DriverId")
            .IsRequired(false);
        builder.HasOne(x => x.Photo)
            .WithOne(x => x.Driver)
            .HasForeignKey<Photo>("DriverId")
            .IsRequired(false);

        // Property Marker -- Deleting or modifying this comment could cause incomplete relationship scaffolding
        
        // example for a more complex value object
        // builder.OwnsOne(x => x.PhysicalAddress, opts =>
        // {
        //     opts.Property(x => x.Line1).HasColumnName("physical_address_line1");
        //     opts.Property(x => x.Line2).HasColumnName("physical_address_line2");
        //     opts.Property(x => x.City).HasColumnName("physical_address_city");
        //     opts.Property(x => x.State).HasColumnName("physical_address_state");
        //     opts.OwnsOne(x => x.PostalCode, o =>
        //         {
        //             o.Property(x => x.Value).HasColumnName("physical_address_postal_code");
        //         }).Navigation(x => x.PostalCode)
        //         .IsRequired();
        //     opts.Property(x => x.Country).HasColumnName("physical_address_country");
        // }).Navigation(x => x.PhysicalAddress)
        //     .IsRequired();
    }
}

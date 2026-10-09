namespace DriversBlogManagement.Databases.EntityConfigurations;

using DriversBlogManagement.Domain.Drivers;
using DriversBlogManagement.Domain.PostAboutDrivers;
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
        // (HasForeignKey<Driver>(s => s.Id)), тобто Driver.Id мусив дорівнювати Id PostAboutDriver.
        // На практиці це унеможливлювало вставку: EF Core під час fix-up підміняв Id нового
        // запису (або ловив порушення FK). Тепер залежною стороною є PostAboutDriver, а зв'язок
        // тримає окрема опційна колонка driver_id (snake_case від тіньової властивості DriverId).
        // Для тіньового водія це критично: його Id ОБОВ'ЯЗКОВО має збігатися з Id водія
        // у DriversManagement (приходить у події IDriverCreated), інакше реплікація ламається.
        builder.HasOne(x => x.PostAboutDriver)
            .WithOne(x => x.Driver)
            .HasForeignKey<PostAboutDriver>("DriverId")
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

namespace DriversBlogManagement.Databases.EntityConfigurations;

using DriversBlogManagement.Domain.BlogUsers;
using DriversBlogManagement.Domain.PostAboutDrivers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class BlogUserConfiguration : IEntityTypeConfiguration<BlogUser>
{
    /// <summary>
    /// The database configuration for BlogUsers. 
    /// </summary>
    public void Configure(EntityTypeBuilder<BlogUser> builder)
    {
        // Relationship Marker -- Deleting or modifying this comment could cause incomplete relationship scaffolding
        // ⚠️ ВИПРАВЛЕНО: Craftsman згенерував зв'язок "один-до-одного" зі СПІЛЬНИМ первинним ключем
        // (HasForeignKey<BlogUser>(s => s.Id)), тобто BlogUser.Id мусив дорівнювати Id PostAboutDriver.
        // На практиці це унеможливлювало вставку: EF Core під час fix-up підміняв Id нового
        // запису (або ловив порушення FK). Тепер залежною стороною є PostAboutDriver, а зв'язок
        // тримає окрема опційна колонка blog_user_id (snake_case від тіньової властивості BlogUserId).
        builder.HasOne(x => x.PostAboutDriver)
            .WithOne(x => x.BlogUser)
            .HasForeignKey<PostAboutDriver>("BlogUserId")
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

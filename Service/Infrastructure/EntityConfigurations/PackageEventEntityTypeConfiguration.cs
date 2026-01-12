using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Staging.Domain.AggregatesModel.PackageAggregate;

namespace Staging.Infrastructure;

class PackageEventEntityTypeConfiguration : IEntityTypeConfiguration<PackageEvent>
{
    public void Configure(EntityTypeBuilder<PackageEvent> configuration)
    {
        configuration.ToTable("PackageEvent");

        configuration.Ignore(b => b.DomainEvents);

        configuration
            .Property<DateTime?>("Created");
        configuration
            .Property<Guid?>("CreatedId");
        configuration
            .Property<DateTime?>("LastModified");
        configuration
            .Property<Guid?>("LastModifiedId");

        configuration
            .Property(c => c.PackageStatusId)
            .IsRequired();

        configuration
            .Property(c => c.OrgUnitId)
            .IsRequired();

        configuration
            .Property(c => c.OrgUnitName)
            .IsRequired()
            .HasMaxLength(100);

        configuration.HasMany(b => b.Devices)
           .WithOne()
           .HasForeignKey("PackageEventId")
           .OnDelete(DeleteBehavior.Cascade);

        var navigation = configuration.Metadata.FindNavigation(nameof(PackageEvent.Devices));
        navigation.SetPropertyAccessMode(PropertyAccessMode.Field);

        configuration.HasMany(b => b.Households)
           .WithOne()
           .HasForeignKey("PackageEventId")
           .OnDelete(DeleteBehavior.Cascade);

        navigation = configuration.Metadata.FindNavigation(nameof(PackageEvent.Households));
        navigation.SetPropertyAccessMode(PropertyAccessMode.Field);

        configuration.HasMany(b => b.Flags)
           .WithOne()
           .HasForeignKey("PackageEventId")
           .OnDelete(DeleteBehavior.Cascade);

        navigation = configuration.Metadata.FindNavigation(nameof(PackageEvent.Flags));
        navigation.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

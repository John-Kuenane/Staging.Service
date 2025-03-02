using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MISSA.Services.Staging.Domain.AggregatesModel.PackageAggregate;

namespace MISSA.Services.Staging.Infrastructure;

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
            .OwnsOne(o => o.EventOpened, a =>
            {
                a.WithOwner();
            });

        configuration
            .OwnsOne(o => o.DeviceRegisteredStageClosed, a =>
            {
                a.WithOwner();
            });

        configuration
            .OwnsOne(o => o.DataManagementStageClosed, a =>
            {
                a.WithOwner();
            });

        configuration
            .OwnsOne(o => o.DataAcceptanceStageClosed, a =>
            {
                a.WithOwner();
            });

        configuration
            .OwnsOne(o => o.GatewayStageClosed, a =>
            {
                a.WithOwner();
            });

        configuration
            .Property(c => c.OrgUnitId)
            .IsRequired();

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

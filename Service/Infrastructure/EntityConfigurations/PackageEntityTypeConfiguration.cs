using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Staging.Domain.AggregatesModel.PackageAggregate;

namespace Staging.Infrastructure;

class PackageEntityTypeConfiguration : IEntityTypeConfiguration<Package>
{
    public void Configure(EntityTypeBuilder<Package> configuration)
    {
        configuration.ToTable("Package");

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
            .Property(c => c.PackageTypeId)
            .IsRequired();

        configuration
            .OwnsOne(o => o.PackageOpened, a =>
            {
                a.WithOwner();
            });

        configuration
            .OwnsOne(o => o.PackageClosed, a =>
            {
                a.WithOwner();
            });

        configuration
            .Property(c => c.OrgUnitId)
            .IsRequired();

        configuration
            .Property(c => c.ParentOrgunitName)
            .IsRequired()
            .HasMaxLength(500);

        configuration
            .Property(c => c.UniqueCode)
            .IsRequired()
            .HasMaxLength(10);

        configuration
            .Property(c => c.Description)
            .HasMaxLength(500);

        configuration
            .Property(c => c.Vendor)
            .HasMaxLength(20);

        configuration.HasIndex("UniqueCode")
            .IsUnique(true);

        configuration.HasMany(b => b.Events)
           .WithOne()
           .HasForeignKey("PackageId")
           .OnDelete(DeleteBehavior.Cascade);

        var navigation = configuration.Metadata.FindNavigation(nameof(Package.Events));
        navigation.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

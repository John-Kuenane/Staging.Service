using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Staging.Domain.AggregatesModel.PackageAggregate;

namespace Staging.Infrastructure;

class PackageEventHouseholdEntityTypeConfiguration : IEntityTypeConfiguration<PackageEventHousehold>
{
    public void Configure(EntityTypeBuilder<PackageEventHousehold> configuration)
    {
        configuration.ToTable("PackageEventHousehold");

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
            .Property(c => c.HouseholdId)
            .IsRequired();

        configuration
            .Property(c => c.HouseholdGuid)
            .IsRequired();

        configuration
            .Property(c => c.CommunityClassification)
            .IsRequired()
            .HasMaxLength(30);

        configuration
            .Property(c => c.HouseholdHead)
            .IsRequired()
            .HasMaxLength(150);

        configuration
            .Property(c => c.ContactNumber)
            .HasMaxLength(25);

        configuration
            .Property(c => c.PhysicalAddress)
            .HasMaxLength(250);

        configuration
            .Property(c => c.VillageName)
            .IsRequired()
            .HasMaxLength(150);

        configuration
            .Property(c => c.ListingStatusId)
            .IsRequired();

        configuration
            .Property(c => c.CollectionStatusId)
            .IsRequired();

        configuration
            .OwnsOne(o => o.Accepted, a =>
            {
                a.WithOwner();
            });

        configuration
            .OwnsOne(o => o.Rejected, a =>
            {
                a.WithOwner();
            });

        configuration
            .Property(c => c.Comments)
            .HasMaxLength(500);

        //configuration
        //    .HasIndex(c => c.HouseholdId)
        //    .IsUnique()
        //    .HasDatabaseName("UX_PackageEventHousehold_HouseholdId");

        configuration.HasMany(b => b.Members)
           .WithOne()
           .HasForeignKey("PackageEventHouseholdId")
           .OnDelete(DeleteBehavior.Cascade);

        var navigation = configuration.Metadata.FindNavigation(nameof(PackageEventHousehold.Members));
        navigation.SetPropertyAccessMode(PropertyAccessMode.Field);

        configuration.HasMany(b => b.Attributes)
           .WithOne()
           .HasForeignKey("PackageEventHouseholdId")
           .OnDelete(DeleteBehavior.Cascade);

        navigation = configuration.Metadata.FindNavigation(nameof(PackageEventHousehold.Attributes));
        navigation.SetPropertyAccessMode(PropertyAccessMode.Field);

        configuration.HasMany(b => b.Synchs)
           .WithOne()
           .HasForeignKey("PackageEventHouseholdId")
           .OnDelete(DeleteBehavior.Cascade);

        navigation = configuration.Metadata.FindNavigation(nameof(PackageEventHousehold.Synchs));
        navigation.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MISSA.Services.Staging.Domain.AggregatesModel.PackageAggregate;

namespace MISSA.Services.Staging.Infrastructure;

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
            .Property(c => c.Village)
            .IsRequired()
            .HasMaxLength(150);

        configuration
            .Property(c => c.HouseholdHead)
            .IsRequired()
            .HasMaxLength(150);

        configuration
            .Property(c => c.ContactNumber)
            .HasMaxLength(25);

        configuration
            .Property(c => c.Address)
            .HasMaxLength(250);

        configuration
            .OwnsOne(o => o.Enumerated, a =>
            {
                a.WithOwner();
            });

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

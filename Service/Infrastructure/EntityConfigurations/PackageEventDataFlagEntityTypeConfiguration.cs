using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Staging.Domain.AggregatesModel.PackageAggregate;

namespace Staging.Infrastructure;

class PackageEventDataFlagEntityTypeConfiguration : IEntityTypeConfiguration<PackageEventDataFlag>
{
    public void Configure(EntityTypeBuilder<PackageEventDataFlag> configuration)
    {
        configuration.ToTable("PackageEventDataFlag");

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
            .Property(c => c.PackageEventHouseholdId)
            .IsRequired();

        configuration
            .Property(c => c.DataFlagTypeId)
            .IsRequired();

        configuration
            .OwnsOne(o => o.FlagResolved, a =>
            {
                a.WithOwner();
            });

        configuration
            .OwnsOne(o => o.FlagDeferred, a =>
            {
                a.WithOwner();
            });

        configuration
            .OwnsOne(o => o.Requester, a =>
            {
                a.WithOwner();
            });

        configuration
            .Property(c => c.Subject)
            .IsRequired()
            .HasMaxLength(100);

        configuration
            .Property(c => c.Description)
            .IsRequired()
            .HasMaxLength(750);

        configuration
            .Property(c => c.PriorityId)
            .IsRequired();

        configuration.HasMany(b => b.Comments)
           .WithOne()
           .HasForeignKey("PackageEventDataFlagId")
           .OnDelete(DeleteBehavior.Cascade);

        var navigation = configuration.Metadata.FindNavigation(nameof(PackageEventDataFlag.Comments));
        navigation.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

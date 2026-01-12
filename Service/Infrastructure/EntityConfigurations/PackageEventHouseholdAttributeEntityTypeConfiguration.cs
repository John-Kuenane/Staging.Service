using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Staging.Domain.AggregatesModel.PackageAggregate;

namespace Staging.Infrastructure;

class PackageEventHouseholdAttributeEntityTypeConfiguration : IEntityTypeConfiguration<PackageEventHouseholdAttribute>
{
    public void Configure(EntityTypeBuilder<PackageEventHouseholdAttribute> configuration)
    {
        configuration.ToTable("PackageEventHouseholdAttribute");

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
            .Property(c => c.AttributeKey)
            .IsRequired()
            .HasMaxLength(50);

        configuration
            .OwnsOne(o => o.Original, a =>
            {
                a.WithOwner();
            });

        configuration
            .OwnsOne(o => o.Modified, a =>
            {
                a.WithOwner();
            });
    }
}

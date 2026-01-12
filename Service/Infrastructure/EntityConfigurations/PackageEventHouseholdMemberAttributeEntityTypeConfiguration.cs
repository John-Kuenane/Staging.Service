using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Staging.Domain.AggregatesModel.PackageAggregate;

namespace Staging.Infrastructure;

class PackageEventHouseholdMemberAttributeEntityTypeConfiguration : IEntityTypeConfiguration<PackageEventHouseholdMemberAttribute>
{
    public void Configure(EntityTypeBuilder<PackageEventHouseholdMemberAttribute> configuration)
    {
        configuration.ToTable("PackageEventHouseholdMemberAttribute");

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

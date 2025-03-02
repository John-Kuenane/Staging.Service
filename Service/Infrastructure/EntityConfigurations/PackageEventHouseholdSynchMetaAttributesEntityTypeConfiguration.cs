using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MISSA.Services.Staging.Domain.AggregatesModel.PackageAggregate;

namespace MISSA.Services.Staging.Infrastructure;

class PackageEventHouseholdSynchMetaAttributesEntityTypeConfiguration : IEntityTypeConfiguration<PackageEventHouseholdSynchMetaAttribute>
{
    public void Configure(EntityTypeBuilder<PackageEventHouseholdSynchMetaAttribute> configuration)
    {
        configuration.ToTable("PackageEventHouseholdSynchMetaAttribute");

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
            .Property(c => c.MetaAttributeTypeId)
            .IsRequired();

        configuration
            .Property(c => c.MetaAttributeValue)
            .IsRequired();
    }
}
